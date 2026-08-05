using System.Globalization;
using System.Text.Json;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceAnalysis;

internal static class Program
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "-h" or "--help")
            {
                Console.Error.WriteLine("Usage: SourceAnalysis <calls|transforms|state-machines|closures|methods|patch-caller-check> --in FILE [--out FILE]");
                return args.Length == 0 ? 1 : 0;
            }

            return args[0] switch
            {
                "calls" => ScanCalls(ReadRequiredInput(args, requireOutput: false)),
                "transforms" => ScanTransforms(ReadRequiredInput(args, requireOutput: false)),
                "state-machines" => ScanStateMachines(ReadRequiredInput(args, requireOutput: false)),
                "closures" => ScanClosures(ReadRequiredInput(args, requireOutput: false)),
                "methods" => ScanMethods(ReadRequiredInput(args, requireOutput: false)),
                "patch-caller-check" => PatchCallerCheck(ReadRequiredInput(args, requireOutput: true)),
                _ => Fail($"unknown command: {args[0]}"),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static (string Input, string? Output) ReadRequiredInput(string[] args, bool requireOutput)
    {
        if (args.Length < 3)
            throw new ArgumentException("expected --in FILE");

        string? input = null;
        string? output = null;
        for (var i = 1; i < args.Length; i++)
        {
            var option = args[i];
            if (option is not ("--in" or "--out"))
                throw new ArgumentException($"invalid argument: {option}");
            if (++i == args.Length)
                throw new ArgumentException($"missing value for {option}");

            if (option == "--in")
            {
                if (input is not null)
                    throw new ArgumentException("--in may only be supplied once");
                input = args[i];
            }
            else
            {
                if (output is not null)
                    throw new ArgumentException("--out may only be supplied once");
                output = args[i];
            }
        }

        if (input is null)
            throw new ArgumentException("missing required --in FILE");
        if (!File.Exists(input))
            throw new FileNotFoundException("input file not found", input);
        if (requireOutput && output is null)
            throw new ArgumentException("missing required --out FILE");
        if (!requireOutput && output is not null)
            throw new ArgumentException("--out is only valid for patch-caller-check");
        if (output is not null && !Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(output))!))
            throw new DirectoryNotFoundException($"output directory does not exist: {Path.GetDirectoryName(Path.GetFullPath(output))}");

        return (Path.GetFullPath(input), output is null ? null : Path.GetFullPath(output));
    }

    private static int ScanCalls((string Input, string? Output) files)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(files.Input), path: files.Input);
        foreach (var invocation in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax { Name: GenericNameSyntax generic } access
                || !IsModuleReceiver(access.Expression)
                || !IsSmethod(generic.Identifier.ValueText)
                || generic.TypeArgumentList.Arguments.Count != 1
                || invocation.ArgumentList.Arguments.Count != 1
                || !TryInteger(invocation.ArgumentList.Arguments[0].Expression, out var key))
            {
                continue;
            }

            Write(new
            {
                spanStart = invocation.SpanStart,
                spanLength = invocation.Span.Length,
                method = generic.Identifier.ValueText,
                key,
            });
        }
        return 0;
    }

    private static bool IsModuleReceiver(ExpressionSyntax receiver) =>
        receiver switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText == "_003CModule_003E",
            AliasQualifiedNameSyntax
            {
                Alias.Identifier.ValueText: "global",
                Name.Identifier.ValueText: "_003CModule_003E",
            } => true,
            _ => false,
        };

    private static bool IsSmethod(string name) =>
        name.StartsWith("smethod_", StringComparison.Ordinal)
        && name.Length > "smethod_".Length
        && name["smethod_".Length..].All(char.IsAsciiDigit);

    private static int ScanTransforms((string Input, string? Output) files)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(files.Input), path: files.Input);
        foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
                     .Where(method => IsSmethod(method.Identifier.ValueText)))
        {
            var transform = method.DescendantNodes(ShouldDescend)
                .OfType<AssignmentExpressionSyntax>()
                .Select(ExtractTransform)
                .FirstOrDefault(transform => transform is not null);
            if (transform is not null)
                Write(new { method = method.Identifier.ValueText, a = transform.A, b = transform.B });
        }
        return 0;
    }

    private static Transform? ExtractTransform(AssignmentExpressionSyntax assignment)
    {
        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
            || assignment.Left is not IdentifierNameSyntax { Identifier.ValueText: "id" })
        {
            return null;
        }

        var expression = Unwrap(assignment.Right);
        if (expression is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.ExclusiveOrExpression } xor
            || !TryInteger(Unwrap(xor.Right), out var b))
        {
            return null;
        }

        var multiply = Unwrap(xor.Left);
        if (multiply is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.MultiplyExpression } product
            || !IsId(Unwrap(product.Left))
            || !TryInteger(Unwrap(product.Right), out var a))
        {
            return null;
        }

        return new Transform(a, b);
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (true)
        {
            expression = expression switch
            {
                ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
                CastExpressionSyntax cast => cast.Expression,
                CheckedExpressionSyntax checkedExpression => checkedExpression.Expression,
                _ => expression,
            };
            if (expression is not ParenthesizedExpressionSyntax
                and not CastExpressionSyntax
                and not CheckedExpressionSyntax)
            {
                return expression;
            }
        }
    }

    private static bool IsId(ExpressionSyntax expression) =>
        expression is IdentifierNameSyntax { Identifier.ValueText: "id" };

    private static int ScanStateMachines((string Input, string? Output) files)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(files.Input), path: files.Input);
        foreach (var loop in tree.GetRoot().DescendantNodes().OfType<WhileStatementSyntax>())
        {
            if (!loop.Condition.IsKind(SyntaxKind.TrueLiteralExpression)
                || loop.Statement is not BlockSyntax body)
            {
                continue;
            }

            var dispatcher = body.Statements.OfType<SwitchStatementSyntax>()
                .FirstOrDefault(s => s.Expression is IdentifierNameSyntax);
            if (dispatcher?.Expression is not IdentifierNameSyntax state)
                continue;

            var variable = state.Identifier.ValueText;
            var seed = FindSeed(loop, variable);
            var cases = dispatcher.Sections.Select(section => DescribeCase(section, variable)).ToList();
            var verdict = Classify(cases, seed, out var path);
            var line = tree.GetLineSpan(loop.WhileKeyword.Span).StartLinePosition.Line + 1;

            Write(new
            {
                method = EnclosingMethod(loop),
                whileLine = line,
                variable,
                seed,
                cases,
                verdict,
                path,
            });
        }
        return 0;
    }

    /// <summary>
    /// Reports every compiler-generated closure type left standing in a decompiled file, and every
    /// place a method still constructs one explicitly.
    ///
    /// A closure the decompiler managed to inline leaves no trace: it becomes a lambda and the type
    /// disappears from the output. So everything this reports is, by construction, a closure that was
    /// NOT inlined -- and the fields it carries say why. A static field of the closure's own type is
    /// obfuscator residue that should have been stripped upstream; an instance field whose type is
    /// another closure is a captured parent scope, which is the shape the decompiler is documented to
    /// give up on. Distinguishing those two is the whole point of the record.
    ///
    /// Purely syntactic and per-file, like the other commands here; joining sites to types and
    /// aggregating across a tree is the caller's job.
    /// </summary>
    /// <summary>
    /// Every method declaration in a file with its line span, so a caller can map changed lines back
    /// to the methods that contain them. Filenames and diff hunks alone cannot answer "which methods
    /// changed" -- a hunk sits inside exactly one method, and only a parser knows which.
    /// </summary>
    private static int ScanMethods((string Input, string? Output) files)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(files.Input), path: files.Input);
        foreach (var member in tree.GetRoot().DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
        {
            var span = tree.GetLineSpan(member.Span);
            var owner = member.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            Write(new
            {
                type = owner?.Identifier.ValueText ?? "<none>",
                name = member switch
                {
                    MethodDeclarationSyntax m => m.Identifier.ValueText,
                    ConstructorDeclarationSyntax c => ".ctor",
                    DestructorDeclarationSyntax d => "~" + d.Identifier.ValueText,
                    OperatorDeclarationSyntax o => "operator " + o.OperatorToken.ValueText,
                    ConversionOperatorDeclarationSyntax cv => "operator " + cv.Type,
                    _ => "?",
                },
                startLine = span.StartLinePosition.Line + 1,
                endLine = span.EndLinePosition.Line + 1,
            });
        }
        return 0;
    }

    private static int ScanClosures((string Input, string? Output) files)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(files.Input), path: files.Input);
        var root = tree.GetRoot();

        foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var name = type.Identifier.ValueText;
            if (!IsClosureTypeName(name))
                continue;

            var staticSelfRefFields = new List<string>();
            var capturedClosureFields = new List<object>();
            var instanceFields = 0;

            foreach (var field in type.Members.OfType<FieldDeclarationSyntax>())
            {
                var isStatic = field.Modifiers.Any(SyntaxKind.StaticKeyword);
                var typeName = field.Declaration.Type.ToString();
                foreach (var declarator in field.Declaration.Variables)
                {
                    if (isStatic)
                    {
                        // The C# compiler emits exactly one static *readonly* self-reference per
                        // <>c type -- the singleton delegate cache. That one is legitimate and
                        // DisplayClassCleaner deliberately preserves it. Only a writable static
                        // self-reference is obfuscator residue.
                        if (typeName == name && !field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
                            staticSelfRefFields.Add(declarator.Identifier.ValueText);
                        continue;
                    }

                    instanceFields++;
                    if (IsClosureTypeName(typeName))
                        capturedClosureFields.Add(new { field = declarator.Identifier.ValueText, type = typeName });
                }
            }

            Write(new
            {
                kind = "closureType",
                name,
                line = tree.GetLineSpan(type.Identifier.Span).StartLinePosition.Line + 1,
                instanceFields,
                staticSelfRefFields,
                capturedClosureFields,
                delegateTargets = type.Members.OfType<MethodDeclarationSyntax>()
                    .Where(m => !m.Modifiers.Any(SyntaxKind.StaticKeyword))
                    .Select(m => m.Identifier.ValueText)
                    .ToList(),
            });
        }

        foreach (var creation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            var typeName = creation.Type.ToString();
            if (!IsClosureTypeName(typeName))
                continue;

            Write(new
            {
                kind = "constructionSite",
                closureType = typeName,
                method = EnclosingMethod(creation),
                line = tree.GetLineSpan(creation.Span).StartLinePosition.Line + 1,
            });
        }

        return 0;
    }

    /// <summary>
    /// C# names compiler-generated closures &lt;&gt;c__DisplayClassN_M and the static delegate cache
    /// &lt;&gt;c. ILSpy escapes the angle brackets to _003C_003E, so accept both spellings.
    /// </summary>
    private static bool IsClosureTypeName(string name) =>
        name.StartsWith("_003C_003Ec", StringComparison.Ordinal)
        || name.StartsWith("<>c", StringComparison.Ordinal);

    private static long? FindSeed(WhileStatementSyntax loop, string variable)
    {
        if (loop.Parent is not BlockSyntax block)
            return null;
        var index = block.Statements.IndexOf(loop);
        if (index <= 0 || block.Statements[index - 1] is not LocalDeclarationStatementSyntax declaration)
            return null;

        var declarator = declaration.Declaration.Variables
            .SingleOrDefault(v => v.Identifier.ValueText == variable);
        if (declarator?.Initializer is null)
            return null;
        if (TryInteger(declarator.Initializer.Value, out var value))
            return value;
        return declarator.Initializer.Value is DefaultExpressionSyntax
            or LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression } ? 0 : null;
    }

    private static CaseResult DescribeCase(SwitchSectionSyntax section, string variable)
    {
        var labels = section.Labels.Select(LabelValue).ToArray();
        var descendants = section.DescendantNodes(ShouldDescend).ToArray();
        var nestedOrConditionalWrite = descendants.Any(node => node is SwitchStatementSyntax)
            || descendants.OfType<VariableDeclaratorSyntax>().Any(d => d.Identifier.ValueText == variable);
        var exits = descendants.Any(node => node is ReturnStatementSyntax or ThrowStatementSyntax);
        var targets = new HashSet<long>();

        foreach (var assignment in descendants.OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Left is not IdentifierNameSyntax left || left.Identifier.ValueText != variable)
                continue;
            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                || !TryInteger(assignment.Right, out var target)
                || IsConditional(assignment, section))
            {
                nestedOrConditionalWrite = true;
                continue;
            }
            targets.Add(target);
        }

        if (descendants.OfType<PrefixUnaryExpressionSyntax>()
                .Concat<SyntaxNode>(descendants.OfType<PostfixUnaryExpressionSyntax>())
                .Any(node => WritesVariable(node, variable)))
        {
            nestedOrConditionalWrite = true;
        }

        var outcome = nestedOrConditionalWrite || (exits && targets.Count != 0) || targets.Count > 1
            ? "unknown"
            : exits ? "exit"
            : targets.Count == 1 ? "goto"
            : "unknown";
        return new CaseResult(labels, outcome, targets.Count == 1 ? targets.Single() : null);
    }

    private static bool ShouldDescend(SyntaxNode node) =>
        node is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);

    private static bool WritesVariable(SyntaxNode node, string variable) =>
        node switch
        {
            PrefixUnaryExpressionSyntax { Operand: IdentifierNameSyntax id } => id.Identifier.ValueText == variable,
            PostfixUnaryExpressionSyntax { Operand: IdentifierNameSyntax id } => id.Identifier.ValueText == variable,
            _ => false,
        };

    private static bool IsConditional(SyntaxNode node, SwitchSectionSyntax section) =>
        node.Ancestors().TakeWhile(parent => parent != section).Any(parent =>
            parent is IfStatementSyntax
                or ConditionalExpressionSyntax
                or ForStatementSyntax
                or ForEachStatementSyntax
                or ForEachVariableStatementSyntax
                or WhileStatementSyntax
                or DoStatementSyntax
                or TryStatementSyntax);

    private static object LabelValue(SwitchLabelSyntax label) =>
        label is DefaultSwitchLabelSyntax ? "default"
        : label is CaseSwitchLabelSyntax { Value: var value } && TryInteger(value, out var number) ? number
        : label.ToString();

    private static string EnclosingMethod(SyntaxNode node) =>
        node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault() switch
        {
            MethodDeclarationSyntax method => method.Identifier.ValueText,
            ConstructorDeclarationSyntax constructor => constructor.Identifier.ValueText,
            DestructorDeclarationSyntax destructor => "~" + destructor.Identifier.ValueText,
            OperatorDeclarationSyntax op => "operator " + op.OperatorToken.ValueText,
            ConversionOperatorDeclarationSyntax conversion => "operator " + conversion.Type,
            _ => "?",
        };

    private static string Classify(IReadOnlyCollection<CaseResult> sections, long? seed, out List<long>? path)
    {
        path = seed is null ? null : [];
        if (seed is null)
            return "UNKNOWN";

        var cases = new Dictionary<long, CaseResult>();
        CaseResult? defaultCase = null;
        foreach (var section in sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is long value)
                    cases[value] = section;
                else if (label is string { } text && text == "default")
                    defaultCase = section;
            }
        }

        var seen = new HashSet<long>();
        var state = seed.Value;
        for (var i = 0; i < 512; i++)
        {
            if (!seen.Add(state))
                return "LOOPS";
            path!.Add(state);
            var outcome = cases.GetValueOrDefault(state) ?? defaultCase;
            if (outcome is null || outcome.Outcome == "unknown")
                return "UNKNOWN";
            if (outcome.Outcome == "exit")
                return "TERMINATES";
            if (outcome.Next is null)
                return "UNKNOWN";
            state = outcome.Next.Value;
        }
        return "UNKNOWN";
    }

    private static int PatchCallerCheck((string Input, string? Output) files)
    {
        var output = files.Output!;
        using var module = ModuleDefMD.Load(files.Input);
        var patched = 0;
        foreach (var method in module.GetTypes().SelectMany(type => type.Methods)
                     .Where(method => IsSmethod(method.Name) && method.HasBody))
        {
            var instructions = method.Body.Instructions;

            // Collect matches by *reference* in a first pass, then mutate, so inserting an
            // unconditional branch below cannot invalidate an index we still need. In practice
            // Reactor emits exactly one guard per smethod, but nothing here assumes that.
            var guards = new List<(Instruction Branch, Instruction Body, bool BodyAtTarget)>();
            for (var i = 0; i < instructions.Count; i++)
            {
                var branch = instructions[i];
                if (!IsConditionalBranch(branch) || !IsCallerGuard(instructions, i))
                    continue;

                // The guard is `if (executing.Equals(calling)) <run> else return default`, and the
                // obfuscator emits it in BOTH polarities: `brfalse` past the body to the
                // return-default, and `brtrue` to the body with the return-default falling through.
                // Neutralising blindly with `pop` is only correct for the first shape -- in the
                // second it drops execution straight into the return-default and every call yields
                // default(T). So decide from where the *return-default* actually sits: the body is
                // whichever successor is not it.
                var target = (Instruction)branch.Operand;
                var targetIsFail = IsGuardFailPath(instructions, instructions.IndexOf(target));
                var fallIsFail = IsGuardFailPath(instructions, i + 1);
                if (targetIsFail == fallIsFail)
                    continue;  // can't tell body from fail path; leave it rather than guess wrong

                var bodyAtTarget = fallIsFail;
                var body = bodyAtTarget ? target : instructions[i + 1];
                guards.Add((branch, body, bodyAtTarget));
            }

            foreach (var (branch, body, bodyAtTarget) in guards)
            {
                var offset = branch.Offset;

                // Consume the equality result the branch would have consumed, then reach the body
                // unconditionally. When the body is the fall-through, popping is enough; when it is
                // the branch target, an explicit `br` replaces the conditional jump (dnlib
                // recomputes the offset, so the long form is always safe).
                branch.OpCode = OpCodes.Pop;
                branch.Operand = null;
                string replacement;
                if (bodyAtTarget)
                {
                    instructions.Insert(instructions.IndexOf(branch) + 1,
                        Instruction.Create(OpCodes.Br, body));
                    replacement = "pop+br-to-body";
                }
                else
                {
                    replacement = "pop";
                }

                Write(new { method = method.FullName, offset, replacement });
                patched++;
            }
        }

        if (patched == 0)
            throw new InvalidOperationException("no Assembly caller guards found in smethod_N methods");

        var options = new ModuleWriterOptions(module)
        {
            Logger = DummyLogger.NoThrowInstance,
        };
        options.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
        module.Write(output, options);
        Write(new { patched, output });
        return 0;
    }

    private static bool IsConditionalBranch(Instruction instruction) =>
        instruction.OpCode.Code is Code.Brfalse or Code.Brfalse_S or Code.Brtrue or Code.Brtrue_S;

    // Recognises the guard's return-default arm: a short run ending in `ret` that only loads a
    // default (a zeroed local, null, or a constant) and does none of the arithmetic, indexing or
    // calls the real decryption body opens with. Reactor's arm is `ldloca.s N; initobj T; ldloc.N;
    // ret` for the generic overload and `ldnull; ret` (or `ldc.i4.0; ret`) for the concrete ones.
    private static bool IsGuardFailPath(IList<Instruction> instructions, int start)
    {
        if (start < 0)
            return false;
        for (var i = start; i < instructions.Count && i - start < 8; i++)
        {
            var code = instructions[i].OpCode.Code;
            if (code is Code.Ret)
                return true;
            if (!IsDefaultLoadOrLocal(code))
                return false;  // real work -> this is the body, not the fail path
        }
        return false;
    }

    private static bool IsDefaultLoadOrLocal(Code code) => code is
        Code.Nop or Code.Ldnull or Code.Initobj
        or Code.Ldloc or Code.Ldloc_S or Code.Ldloc_0 or Code.Ldloc_1 or Code.Ldloc_2 or Code.Ldloc_3
        or Code.Ldloca or Code.Ldloca_S
        or Code.Stloc or Code.Stloc_S or Code.Stloc_0 or Code.Stloc_1 or Code.Stloc_2 or Code.Stloc_3
        or Code.Ldc_I4 or Code.Ldc_I4_S or Code.Ldc_I4_0 or Code.Ldc_I4_1 or Code.Ldc_I4_2
        or Code.Ldc_I4_3 or Code.Ldc_I4_4 or Code.Ldc_I4_5 or Code.Ldc_I4_6 or Code.Ldc_I4_7
        or Code.Ldc_I4_8 or Code.Ldc_I4_M1 or Code.Ldstr;

    private static bool IsCallerGuard(IList<Instruction> instructions, int branchIndex)
    {
        var calls = new List<(int Index, IMethod Method)>();
        for (var i = branchIndex - 1; i >= 0 && branchIndex - i <= 12 && calls.Count < 3; i--)
        {
            if (instructions[i].Operand is IMethod method
                && instructions[i].OpCode.Code is Code.Call or Code.Callvirt)
            {
                calls.Add((i, method));
            }
        }
        if (calls.Count != 3)
            return false;

        calls.Reverse();
        return CallsAssemblyMethod(calls[0].Method, "GetExecutingAssembly")
            && CallsAssemblyMethod(calls[1].Method, "GetCallingAssembly")
            && CallsAssemblyEquality(calls[2].Method)
            && OnlyAssemblyProxyLoads(instructions, calls[0].Index + 1, calls[1].Index)
            && OnlyAssemblyProxyLoads(instructions, calls[1].Index + 1, calls[2].Index)
            && EqualityFeedsBranch(instructions, calls[2].Index + 1, branchIndex);
    }

    private static bool OnlyAssemblyProxyLoads(IList<Instruction> instructions, int start, int end) =>
        Enumerable.Range(start, end - start).All(index =>
            instructions[index].OpCode.Code is Code.Ldsfld or Code.Nop);

    private static bool EqualityFeedsBranch(IList<Instruction> instructions, int start, int end)
    {
        var between = Enumerable.Range(start, end - start)
            .Select(index => instructions[index])
            .Where(instruction => instruction.OpCode.Code != Code.Nop)
            .ToArray();
        return between.Length == 0
            || between.Length == 2
            && TryGetLocalSlot(between[0], store: true, out var stored)
            && TryGetLocalSlot(between[1], store: false, out var loaded)
            && stored == loaded;
    }

    private static bool TryGetLocalSlot(Instruction instruction, bool store, out int slot)
    {
        slot = instruction.OpCode.Code switch
        {
            Code.Stloc_0 when store => 0,
            Code.Stloc_1 when store => 1,
            Code.Stloc_2 when store => 2,
            Code.Stloc_3 when store => 3,
            Code.Ldloc_0 when !store => 0,
            Code.Ldloc_1 when !store => 1,
            Code.Ldloc_2 when !store => 2,
            Code.Ldloc_3 when !store => 3,
            _ when instruction.OpCode.Code is Code.Stloc or Code.Stloc_S && store
                && instruction.Operand is Local local => local.Index,
            _ when instruction.OpCode.Code is Code.Ldloc or Code.Ldloc_S && !store
                && instruction.Operand is Local local => local.Index,
            _ => -1,
        };
        return slot >= 0;
    }

    private static bool CallsAssemblyMethod(IMethod method, string name) =>
        IsAssemblyMethod(method, name)
        || method.ResolveMethodDef()?.Body?.Instructions.Any(instruction =>
            instruction.Operand is IMethod called && IsAssemblyMethod(called, name)) == true;

    private static bool CallsAssemblyEquality(IMethod method)
    {
        if (IsAssemblyEquality(method))
            return true;
        var definition = method.ResolveMethodDef();
        return definition?.ReturnType.ElementType == ElementType.Boolean
            && definition.Body?.Instructions.Any(instruction =>
                instruction.Operand is IMethod called && IsAssemblyEquality(called)) == true;
    }

    private static bool IsAssemblyMethod(IMethod method, string name) =>
        method.Name.String == name && method.DeclaringType?.FullName == "System.Reflection.Assembly";

    private static bool IsAssemblyEquality(IMethod method) =>
        method.Name.String is "Equals" or "op_Equality"
        && method.DeclaringType?.FullName is "System.Reflection.Assembly" or "System.Object";

    private static bool TryInteger(ExpressionSyntax expression, out long value)
    {
        if (expression is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression } negative
            && TryInteger(negative.Operand, out value))
        {
            value = -value;
            return true;
        }
        if (expression is not LiteralExpressionSyntax literal
            || !literal.IsKind(SyntaxKind.NumericLiteralExpression)
            || literal.Token.Value is null)
        {
            value = default;
            return false;
        }

        try
        {
            value = Convert.ToInt64(literal.Token.Value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (OverflowException)
        {
            value = default;
            return false;
        }
    }

    private static void Write<T>(T value) => Console.WriteLine(JsonSerializer.Serialize(value, Json));

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"error: {message}");
        return 1;
    }

    private sealed record CaseResult(object[] Labels, string Outcome, long? Next);

    private sealed record Transform(long A, long B);
}
