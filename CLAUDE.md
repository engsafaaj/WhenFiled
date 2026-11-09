# CLAUDE.md

WhenFiled is a Windows 11 WPF app, kept small on purpose as a teaching example. It moves photos and videos copied from a phone into `Year\Month` folders by the date each was taken, shows where each date came from, and can undo the move.

- What to build: [docs/spec.md](docs/spec.md)
- How it is built, and why: [docs/decisions.md](docs/decisions.md)
- What to do next: [docs/plan.md](docs/plan.md)
- UI mock: [design/mock.html](design/mock.html) (open it in a browser)

## Commands

Run these from the repo root. They need the .NET 10 SDK.

```sh
dotnet build WhenFiled.slnx                     # build everything
dotnet test WhenFiled.slnx                      # run all tests
dotnet test WhenFiled.slnx --filter "FullyQualifiedName~FileNameDate"   # one test class
dotnet test WhenFiled.slnx --filter "FullyQualifiedName~MoverTests"     # the temp-folder move tests
dotnet run --project src/WhenFiled.App          # start the app
```

Task T1 checks that these commands work. If one doesn't, fix this section.

## Folder layout

```text
src/WhenFiled.Core/Rules/   pure rules: dates, targets, names, reasons, undo log, summary text
src/WhenFiled.Core/Disk/    the only disk code: Scanner (reads) and Mover (moves and undo)
src/WhenFiled.App/          the WPF window (MainWindow, code-behind)
tests/WhenFiled.Tests/      unit tests for the rules, plus a few temp-folder tests
docs/                       spec, research, decisions, plan
design/mock.html            UI mock
```

References: App → Core and Tests → Core. Core's only package is MetadataExtractor.

## Rules

1. **Never delete a photo.** No code deletes, replaces or copies over a user's file. The only delete allowed is the 1-argument `Directory.Delete` in `Mover`'s Undo, on an empty folder that the last Sort created. Tests delete only their own temp folders.
2. **Never overwrite a file.**
   - Move only with the 2-argument `File.Move(source, target)`, which throws if the target exists.
   - Open user files only with `FileMode.Open` and `FileAccess.Read`.
   - Never change a file's bytes, times or attributes.
3. **Tests first for core logic.** For anything in `Core/Rules/`, write the test, run it and see it fail, then write the code that makes it pass.
4. **Write files in small edits, because the connection is slow.** Create a new file with a short first part, then add the rest with Edit, about 40 lines or fewer per call. Change existing files with Edit, never by rewriting the whole file.
5. **Keep the rules pure.** Code in `Core/Rules/` never touches the disk, the UI, the clock or the local time zone, so pass `today` and the time zone in. Disk calls go only in `Scanner` and `Mover`.
6. **Use temp folders only.** Tests and manual Sort runs use temp test folders, never real photos.
7. **Do one task at a time.** Work through [docs/plan.md](docs/plan.md) in order. Tick a task when its test passes and the build has no warnings.
8. **Record decisions.** A new or changed decision goes in [docs/decisions.md](docs/decisions.md) with its reason.
9. **Stay small and in scope.** The spec's "Will NOT do in v1" list is out of scope. Ask before adding a project, a package, a framework or anything from that list.
10. **Use the mock's wording** for UI text.
