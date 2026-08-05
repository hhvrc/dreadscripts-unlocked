// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static RemoveList      -> EnsureDirectoryExists,   line 7003
//   static InstantiateList -> PrepareAssetPath(string, bool, PathOption), line 7011
//   static AwakeList       -> PrepareAssetPath(string, string, bool),     line 7053
//   static ResetList       -> PrepareSiblingAssetPath, line 7066
//   static FlushList       -> SanitizePath,            line 7081
//   static ConnectList     -> SanitizeFolderPath,      line 7103
//   static CalculateList   -> SanitizeFileName,        line 7115
//   static OrderRules<T>   -> DuplicateAssetTo<T>,     line 4175
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// This is the whole asset-path family: the three sanitisers, the two "give me a usable path"
// entry points built on them, the sibling-of-an-existing-asset convenience, and the bare
// directory guard. DuplicateAssetTo lives elsewhere in the decompiled file (it sits among the
// asset helpers at line 4175 rather than with the path block at 7003-7123) but is included
// here because it exists only to consume a path produced above -- both of its call sites are
// PrepareAssetPath results -- and the menu/parameter asset creators need the pair together.
//
// SIDE EFFECTS ON DISK. Read before calling:
//   * PrepareAssetPath creates directories. On the file branch it creates the containing folder
//     if missing; on the folder branch it creates the folder itself. Both follow up with
//     AssetDatabase.ImportAsset so the new folder appears in the Project window without a
//     refresh. Nothing else is created: no file is written, and no existing file or folder is
//     ever deleted or truncated.
//   * Nothing here overwrites. The returned path may well name an existing asset, and the
//     caller's AssetDatabase.CreateAsset would then replace it -- pass makeUnique to get an
//     unused path instead.
//   * DuplicateAssetTo writes: it creates an asset at the path it is given, replacing whatever
//     was there.
//
// Sanitisation, precisely: SanitizeFileName replaces every character in
// Path.GetInvalidFileNameChars() with '-' (that set includes the separators '/' and '\', so a
// name cannot smuggle in a directory), and turns an empty name into "Unnamed".
// SanitizeFolderPath normalises '\' to '/' and replaces every character in
// Path.GetInvalidPathChars() with '-', per segment. Casing, spaces, dots and leading/trailing
// whitespace are left alone; this rejects characters the filesystem cannot store, it does not
// enforce a naming style.
//
// Paths outside Assets/: this family does NOT agree with EditorUtils.AssetButtons'
// Audit status: VERIFIED against reverse-engineering/export/
// CreateAssetViaSavePanel, which warns and refuses. Here, a *file* path whose directory is
// neither absolute-under-the-project nor already "Assets"-rooted is silently relocated by
// prefixing "Assets/", and a *folder* path is not checked at all -- PrepareAssetPath with
// ForceFolder will happily create a directory anywhere the process can write. The asymmetry is
// in the original; it is preserved. Callers that must stay inside the project should validate
// before calling rather than relying on this.

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Creates <paramref name="path"/> as a directory if it does not already exist.
        /// </summary>
        /// <remarks>
        /// Deliberately plain: no sanitising, no asset import. It is for paths that are already
        /// known good, such as one just returned from <see cref="PrepareAssetPath(string, bool, PathOption)"/>.
        /// </remarks>
        internal static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Turns a path the user or a caller supplied into one an asset can actually be written to:
        /// illegal characters removed, the location forced under <c>Assets</c> where necessary, and
        /// the containing folder created on disk and imported.
        /// </summary>
        /// <param name="path">The path to prepare. Read as a file or a folder per <paramref name="option"/>.</param>
        /// <param name="makeUnique">
        /// Whether to push the result off any existing asset via
        /// <see cref="AssetDatabase.GenerateUniqueAssetPath"/>. Leave false to let the caller
        /// overwrite what is already there.
        /// </param>
        /// <param name="option">Whether <paramref name="path"/> names a file, a folder, or should be guessed at.</param>
        /// <returns>The prepared path, with its parent directory guaranteed to exist.</returns>
        /// <remarks>
        /// <para>
        /// The two branches differ in more than bookkeeping. A file path is *relocated* if its
        /// directory sits outside the project -- "Foo/bar.anim" becomes "Assets/Foo/bar.anim" --
        /// whereas a folder path is taken at face value and created wherever it points, including
        /// outside <c>Assets</c>. Only the file branch checks at all.
        /// </para>
        /// <para>
        /// The uniquifying differs too: on the file branch the unique path is merely returned, since
        /// the caller has yet to create the file, while on the folder branch the unique folder is
        /// created immediately -- and note that this happens only when the folder already existed.
        /// A missing folder is created under its requested name even with
        /// <paramref name="makeUnique"/> set, which is the sensible reading of "make sure I get a
        /// folder of my own".
        /// </para>
        /// <para>
        /// <see cref="Path.GetDirectoryName"/> returns the platform separator, so on Windows the
        /// result of the file branch is mixed ("Assets\Foo/bar.anim"). Unity's asset APIs accept
        /// that, and it is what the shipped tool produces, so it is left as is.
        /// </para>
        /// </remarks>
        internal static string PrepareAssetPath(string path, bool makeUnique = false, PathOption option = PathOption.Normal)
        {
            bool forceFolder = option == PathOption.ForceFolder;
            bool forceFile = option == PathOption.ForceFile;

            if (forceFile)
            {
                path = SanitizeFileName(path);
            }
            else if (forceFolder)
            {
                path = SanitizeFolderPath(path);
            }
            else
            {
                path = SanitizePath(path);
            }

            // "Guess" means: an extension makes it a file. Note that the guess runs on the already
            // sanitised path, so a dot the sanitiser replaced cannot make a folder look like a file.
            if (!forceFolder && (forceFile || !string.IsNullOrEmpty(Path.GetExtension(path))))
            {
                string directory = Path.GetDirectoryName(path);
                string fileName = Path.GetFileName(path);

                if (string.IsNullOrEmpty(directory))
                {
                    directory = "Assets";
                }
                else if (!directory.StartsWith(Application.dataPath) && !directory.StartsWith("Assets"))
                {
                    // An absolute path into this project's Assets folder is already where it should
                    // be; anything else is assumed to be a project-relative fragment that lost its
                    // root, rather than a deliberate location outside the project.
                    directory = "Assets/" + directory;
                }

                // "Assets" itself is skipped rather than tested: it always exists, and asking the
                // AssetDatabase to import the project root is not free.
                if (directory != "Assets" && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.ImportAsset(directory);
                }

                path = directory + "/" + fileName;

                if (makeUnique)
                {
                    path = AssetDatabase.GenerateUniqueAssetPath(path);
                }
            }
            else if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.ImportAsset(path);
            }
            else if (makeUnique)
            {
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                Directory.CreateDirectory(path);
                AssetDatabase.ImportAsset(path);
            }

            return path;
        }

        /// <summary>
        /// Prepares a path from a folder and a file name given separately, which is how callers that
        /// build a name from an object usually have them.
        /// </summary>
        /// <param name="folder">Destination folder. Empty means "directly under Assets".</param>
        /// <param name="fileName">
        /// File name including extension. Empty means the caller only wants the folder prepared, and
        /// the result is the folder path.
        /// </param>
        /// <param name="makeUnique">As <see cref="PrepareAssetPath(string, bool, PathOption)"/>.</param>
        /// <remarks>
        /// The two halves are sanitised separately before being joined -- that is the point of this
        /// overload, since it lets a file name containing '/' be flattened instead of silently
        /// becoming a subfolder. The joined path then goes through the inferring path again, so a
        /// <paramref name="fileName"/> with no extension is treated as a folder.
        /// </remarks>
        internal static string PrepareAssetPath(string folder, string fileName, bool makeUnique = false)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    return PrepareAssetPath(SanitizeFolderPath(folder) + "/" + SanitizeFileName(fileName), makeUnique);
                }

                return PrepareAssetPath(SanitizeFileName(fileName), makeUnique, PathOption.ForceFile);
            }

            return PrepareAssetPath(SanitizeFolderPath(folder), makeUnique, PathOption.ForceFolder);
        }

        /// <summary>
        /// Prepares a path in the same folder as an existing asset, for writing something alongside
        /// it -- an extracted clip next to its controller, say.
        /// </summary>
        /// <param name="asset">The asset whose folder to write into.</param>
        /// <param name="fileName">
        /// Name for the new file. Empty reuses <paramref name="asset"/>'s own file name, and a bare
        /// extension (".anim") is prefixed with the asset's name.
        /// </param>
        /// <param name="makeUnique">
        /// Defaults to true here, unlike the methods above: writing next to an existing asset almost
        /// always means adding a file rather than replacing one.
        /// </param>
        internal static string PrepareSiblingAssetPath(Object asset, string fileName = "", bool makeUnique = true)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string directory = Path.GetDirectoryName(assetPath);

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetFileName(assetPath);
            }

            if (fileName.StartsWith("."))
            {
                fileName = (!string.IsNullOrEmpty(asset.name) ? asset.name : "SomeAsset") + fileName;
            }

            return PrepareAssetPath(directory, fileName, makeUnique);
        }

        /// <summary>
        /// Makes an arbitrary path legal without deciding for the caller whether it is a file or a
        /// folder: an extension means the last segment is a file name and is sanitised as one, no
        /// extension means the whole string is a folder path.
        /// </summary>
        /// <remarks>
        /// The extension is spliced back on untouched. It came from
        /// <see cref="Path.GetExtension"/>, so it cannot contain a separator, and preserving it
        /// exactly matters -- an asset with a mangled extension is a different asset type to Unity.
        /// </remarks>
        internal static string SanitizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Legalizing empty path! Returned path as 'EmptyPath'");
                return "EmptyPath";
            }

            string extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                return SanitizeFolderPath(path);
            }

            string directory = Path.GetDirectoryName(path);
            string fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(path));

            if (!string.IsNullOrEmpty(directory))
            {
                return SanitizeFolderPath(directory) + "/" + fileName + extension;
            }

            return fileName + extension;
        }

        /// <summary>
        /// Normalises separators to '/' and replaces characters no path may contain with '-'.
        /// </summary>
        /// <remarks>
        /// The replacement runs per segment rather than over the whole string so that '/' survives
        /// it, and it only runs at all when the string actually contains a separator past the first
        /// character. A single-segment name is therefore returned untouched, and so is a path
        /// starting with '/' -- both are relied on by callers that sanitise the pieces themselves
        /// before joining them, so the behaviour is kept as shipped.
        /// </remarks>
        internal static string SanitizeFolderPath(string path)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));

            path = path.Replace('\\', '/');
            if (path.IndexOf('/') > 0)
            {
                path = string.Join("/", path.Split('/').Select(segment => Regex.Replace(segment, "[" + invalidChars + "]", "-")));
            }

            return path;
        }

        /// <summary>
        /// Replaces characters no file name may contain with '-', and substitutes "Unnamed" for an
        /// empty name.
        /// </summary>
        /// <remarks>
        /// The invalid set includes both separators, so the result is always a single path segment:
        /// a name derived from user input cannot escape the folder it was meant for.
        /// </remarks>
        internal static string SanitizeFileName(string fileName)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));

            if (string.IsNullOrEmpty(fileName))
            {
                return "Unnamed";
            }

            return Regex.Replace(fileName, "[" + invalidChars + "]", "-");
        }

        /// <summary>
        /// Copies <paramref name="asset"/> to <paramref name="path"/> and returns the copy, so the
        /// caller can go on editing a private duplicate instead of the shared original.
        /// </summary>
        /// <returns>
        /// The new asset, or null if <paramref name="asset"/> is not saved in the project at all --
        /// there is nothing to copy from in that case.
        /// </returns>
        /// <remarks>
        /// The branch distinguishes a sub-asset from a main asset. Copying the file would drag every
        /// other sub-asset along with it, so a sub-asset is instead instantiated in memory and saved
        /// as a new main asset; a main asset is copied file-and-all, which preserves its own
        /// sub-assets and its import settings. Either way the destination is written, replacing any
        /// asset already at <paramref name="path"/>.
        /// </remarks>
        internal static T DuplicateAssetTo<T>(T asset, string path) where T : Object
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Object mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (!mainAsset)
            {
                return null;
            }

            if (asset != mainAsset)
            {
                T copy = Object.Instantiate(asset);
                AssetDatabase.CreateAsset(copy, path);
                return copy;
            }

            AssetDatabase.CopyAsset(assetPath, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
