# Changelog

## 1.0.0 - 2026-11-09

First release.

- Scan a source folder (subfolders included) and preview each file's date, where the date came from, and its target folder.
- Dates come from EXIF, then video creation time, then the file name. Otherwise the file stays where it is.
- Sort moves files into `Destination\YYYY\MM\`, never overwriting (a ` (2)` is added on a name clash), and lists any file it did not move with the reason.
- Undo last sort moves files back; files that could not go back are kept so Undo can be retried.
- Fixes before release: non-ASCII digits in file names, corrupt-file parser errors, identical duplicates reported as Already in destination, singular wording for counts of 1.
