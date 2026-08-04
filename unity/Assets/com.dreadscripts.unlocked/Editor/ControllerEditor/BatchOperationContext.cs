// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/BatchOperationContext.cs

using System;
using System.Text;
using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Runs the steps of a long editor operation, showing a progress bar and turning an exception in
    /// any step into a report the user can copy and send on.
    /// </summary>
    /// <remarks>
    /// Every method returns the context so a run reads as a chain:
    /// <code>
    /// ctx = ctx.WithTitle("Building").WithDetail(layer.name).ShowProgress().Run(() =&gt; Build(layer)).Step();
    /// </code>
    /// This is a struct, so each of those returns a <em>copy</em>. The result has to be assigned back
    /// — dropping it also drops the step counter and <see cref="hasError"/>.
    /// </remarks>
    internal struct BatchOperationContext
    {
        private StringBuilder errorLog;

        internal int currentStep;

        internal int totalSteps;

        /// <summary>Optional prefix identifying the overall job in error reports.</summary>
        internal string contextName;

        private string operation;

        private string title;

        private string detail;

        /// <summary>
        /// When true a failing step is logged and the run continues; when false the user is shown the
        /// report and the exception is rethrown.
        /// </summary>
        private bool suppressDialog;

        internal bool progressBarShown;

        internal bool hasError;

        /// <summary>Runs one step, capturing any exception it throws.</summary>
        internal BatchOperationContext Run(Action step)
        {
            if (errorLog == null)
            {
                errorLog = new StringBuilder();
            }

            try
            {
                step();
            }
            catch (Exception exception)
            {
                hasError = true;

                string where = operation + " - " + title + " - " + detail + "\n" + exception.Message;
                if (!string.IsNullOrEmpty(contextName))
                {
                    where = contextName + " - " + where;
                }

                errorLog.AppendLine("Error occured at step:\n" + where + "\n\n");

                if (!suppressDialog)
                {
                    if (EditorUtility.DisplayDialog(
                            "Uh oh",
                            $"Something went wrong!\n\n{errorLog}Press Copy and send it to whoever is responsible for this.",
                            "Copy", "Heck"))
                    {
                        EditorGUIUtility.systemCopyBuffer = errorLog.ToString();
                    }

                    throw;
                }
            }
            finally
            {
                if (progressBarShown)
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            return this;
        }

        /// <summary>Sets the progress bar title, also used as the middle field of an error report.</summary>
        internal BatchOperationContext WithTitle(string title)
        {
            this.title = title;
            return this;
        }

        /// <summary>Sets the progress bar detail line, naming the item currently being worked on.</summary>
        internal BatchOperationContext WithDetail(string detail)
        {
            this.detail = detail;
            return this;
        }

        /// <summary>Names the kind of work being done, for error reports.</summary>
        internal BatchOperationContext WithOperation(string operation)
        {
            this.operation = operation;
            return this;
        }

        /// <summary>Advances the step counter that drives the progress bar.</summary>
        internal BatchOperationContext Step()
        {
            currentStep++;
            return this;
        }

        internal BatchOperationContext ShowProgress()
        {
            progressBarShown = true;
            EditorUtility.DisplayProgressBar(title, $"{detail} ({currentStep}/{totalSteps})",
                                             (float)currentStep / totalSteps);
            return this;
        }

        /// <summary>Clears the accumulated state so the context can drive another run.</summary>
        internal BatchOperationContext Reset()
        {
            title = detail = operation = string.Empty;
            currentStep = 0;
            errorLog?.Clear();
            hasError = false;

            // DEOBF-BUG(guessed): export/ loops while the progress-bar flag is set and never clears
            // it, so any reset after a progress bar had been shown would hang the editor. The
            // `while` itself is the de4dot fault confirmed against the original IL on
            // AnimatorTypeCache.ParameterEntry.Source, so reading it as `if` is safe. The
            // `progressBarShown = false` assignment is the guessed part: nothing in export/ shows
            // where the flag was cleared, and without it the guard could never re-arm. Re-derive
            // from export/ if de4dot's control-flow recovery is fixed.
            if (progressBarShown)
            {
                EditorUtility.ClearProgressBar();
                progressBarShown = false;
            }

            return this;
        }

        /// <summary>See <see cref="suppressDialog"/>.</summary>
        internal BatchOperationContext SuppressDialog(bool suppress)
        {
            suppressDialog = suppress;
            return this;
        }
    }
}
