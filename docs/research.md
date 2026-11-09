# Idea research: which small Windows app to build

Researched 2026-11-09. Three candidates: a clipboard history manager, a bulk file renamer, and a phone-photo sorter that files photos into Year/Month folders by date taken.

## Verdict

- **Clipboard history manager: dropped.** Windows' Win+V covers casual use. Ditto and CopyQ are free, open source and well known, and they already fix Win+V's real gaps (unlimited history, search, survives reboot).
- **Bulk file renamer: dropped.** PowerToys PowerRename covers casual use. Bulk Rename Utility, Advanced Renamer and ReNamer cover power use for free (personal use). The complaints are about PowerRename's bugs and UX, not a missing tool.
- **Phone-photo sorter: survives, narrowly.** Sorting photos that have EXIF dates is well covered. Phone dumps are the gap: WhatsApp images, screenshots and some videos have no EXIF date. Among free tools, only paid tiers (PhotoMove Pro, FolioSort Pro) or a command-line tool (ExifTool) handle these files well.

## Comparison at a glance

| | Clipboard history | Bulk renamer | Photo sorter (Year/Month) |
|---|---|---|---|
| Built into Windows | Win+V: 25 items, 4 MB per item, wiped on restart unless pinned | Explorer F2 batch rename (basic numbering only) | Classic "Import pictures and videos" wizard: only for a connected device or drive, flat YYYY-MM folders. Photos app makes no date folders |
| Microsoft PowerToys | Advanced Paste, built on Win+V history (same limits) | PowerRename: regex, preview, undo, date tokens | Nothing |
| Popular free tools | Ditto, CopyQ (free, open source, unlimited) | Bulk Rename Utility, Advanced Renamer, ReNamer (free for personal use) | PhotoMove Free; ExifTool (command line); FolioSort free tier (100 files per batch) |
| Main open complaint | Win+V is too limited; Ditto looks dated | PowerRename bugs (reported 20-file cap, broken Date Taken token), hard for beginners | Files with no EXIF date end up in the wrong month or don't move at all |
| Room for a new small tool? | No | No | Yes, if it targets phone dumps |

## 1. Clipboard history manager (dropped)

**What exists**
- **Windows Win+V.** Off by default. Keeps 25 items, each 4 MB or smaller, in text, HTML or bitmap format only. Unpinned items are wiped on restart. There is no setting, policy or registry value to raise the limit. Microsoft's 2026 support article still describes the same feature that first shipped. Sources: [Copychest: Windows clipboard limits](https://copychest.com/windows-clipboard-history-limits), [EdgeDrop: why only 25 items](https://www.edgedrop.app/blog/windows-clipboard-history-limit-why-only-25-items), [EdgeDrop: Win+V in 2026](https://www.edgedrop.app/blog/win-plus-v-on-windows-11-in-2026-what-still-has-not-changed), [Microsoft forum archive (Win10 1809)](https://learn.microsoft.com/en-us/archive/msdn-technet-forums/bf74f4e6-383a-4c27-b21e-cb2a169e55ce).
- **PowerToys Advanced Paste** (Win+Shift+V). Pastes as plain text, Markdown or JSON, with optional AI actions. Its "clipboard history" view reads Windows' own history, so it has the same 25-item limit and is wiped on reboot. Sources: [Microsoft Learn: Advanced Paste](https://learn.microsoft.com/en-us/windows/powertoys/advanced-paste), [XDA: copy-paste on Windows](https://www.xda-developers.com/5-ways-to-take-copying-and-pasting-to-the-next-level-on-windows/), [Automata Labs comparison](https://automatalabs.ca/blog/powertoys-advanced-paste-vs-intelliclip/), [PowerToys issue #43023](https://github.com/microsoft/PowerToys/issues/43023).
- **Ditto.** Free and open source (GPL-3.0). Unlimited history saved to a database, search, a hotkey, sync between PCs, no account and no telemetry. Installs with winget or from the Microsoft Store. Sources: [GitHub sabrogden/Ditto](https://github.com/sabrogden/Ditto), [How-To Geek](https://www.howtogeek.com/this-open-source-clipboard-manager-slowly-became-my-most-important-productivity-tool/), [MakeUseOf: 6 managers tested](https://www.makeuseof.com/ditto-open-source-clipboard-manager-windows/).
- **CopyQ.** Free and open source, scriptable, cross-platform. Usually listed as the top alternative to Ditto. Sources: [AlternativeTo: Ditto alternatives](https://www.alternativeto.net/software/ditto/), [DEV: free clipboard managers 2026](https://dev.to/abhijith_p_subash/best-free-open-source-clipboard-managers-windows-macos-linux-3f6).

**What users still complain about**
- Win+V: the 25-item cap and losing history on restart (sources above).
- Ditto: mostly that the interface looks dated. A few people report startup problems or can't find features (for example [Ditto issue #574](https://github.com/sabrogden/ditto/issues/574)). Reviews are otherwise strongly positive. Sources: [MakeUseOf](https://www.makeuseof.com/ditto-open-source-clipboard-manager-windows/), [AlternativeTo: about Ditto](https://alternativeto.net/software/ditto/about).

**Why it's dropped:** every real gap in Win+V is already fixed by a free, mature, well-known tool. The only open complaint is how Ditto looks, and a prettier copy of Ditto adds nothing real. A clipboard manager is also an always-running app that hooks system-wide events, which is harder than it looks for a teaching example.

## 2. Bulk file renamer (dropped)

**What exists**
- **PowerToys PowerRename.** Right-click files in Explorer. Search and replace with regex (ECMAScript grammar, or Boost for lookbehind), a live preview, undo, numbering, date tokens, filters for files or folders, and saved recent patterns. Sources: [Microsoft Learn: PowerRename](https://learn.microsoft.com/en-us/windows/powertoys/powerrename), [PowerToys dev docs](https://microsoft.github.io/PowerToys/modules/powerrename/), [MakeUseOf guide](https://www.makeuseof.com/batch-rename-files-powertoys-powerrename/).
- **Bulk Rename Utility.** Free for personal use; a commercial licence is needed for business. Regex, JavaScript, EXIF and ID3 tags, 100+ rules, a portable version and a command-line version. Sources: [bulkrenameutility.co.uk](https://www.bulkrenameutility.co.uk/), [TechSpot download](https://www.techspot.com/downloads/4789-bulk-rename-utility.html).
- **Advanced Renamer.** Free for personal use, easier than Bulk Rename Utility, and renames by EXIF and ID3 tags. Note that the Microsoft Store version is a 10-file trial. Sources: [SourceForge listing](https://sourceforge.net/app/advanced-renamer/), [Microsoft Store listing](https://apps.microsoft.com/detail/9pph77swk13x?hl=en-US&gl=US), [XDA: 4 ways to batch rename](https://www.xda-developers.com/ways-to-batch-rename-files-in-windows-faster-than-ever/).
- **Others:** ReNamer, Ant Renamer, Lupas Rename. Source: [Techcult: 16 free renamers](https://techcult.com/best-free-file-rename-software-for-windows/).

**What users still complain about** (all from PowerToys GitHub issues)
- A reported 20-file limit: [#35866](https://github.com/microsoft/PowerToys/issues/35866) (open, no maintainer explanation yet) and [#41488](https://github.com/microsoft/powertoys/issues/41488).
- It's hard to use. "Completely unintuitive" ([#43815](https://github.com/microsoft/PowerToys/issues/43815)), and a request for a "Simple Mode" ([#49993](https://github.com/microsoft/PowerToys/issues/49993)).
- Bugs. The Date Taken token renames 0 files ([#49517](https://github.com/microsoft/PowerToys/issues/49517)). Counter plus regex can produce name clashes ([#41950](https://github.com/microsoft/powertoys/issues/41950)). Long paths are skipped ([#19887](https://github.com/microsoft/PowerToys/issues/19887)).
- Bulk Rename Utility's interface is "way too intimidating" for first-time users ([XDA](https://www.xda-developers.com/ways-to-batch-rename-files-in-windows-faster-than-ever/)).

**Why it's dropped:** these complaints are bugs and UX polish inside tools that already exist and are under active work (PowerRename gets fixes every release). Anyone hitting the 20-file cap can switch to Advanced Renamer or Bulk Rename Utility for free. A "simple renamer" would sit between PowerRename and Advanced Renamer with no clear reason to exist.

## 3. Phone-photo sorter into Year/Month folders (survives)

**What exists**
- **Windows File Explorer and the Photos app.** Both only sort or group the *view*; neither moves files. The new Photos app (OneDrive Photos, rolling out October to December 2026) lets you browse by date. No source says it files photos into date folders. Sources: [Microsoft Q&A: sort by month](https://learn.microsoft.com/en-us/answers/questions/3294898/sort-by-month-how-do-i-arrange-my-photos-by-month), [Windows blog: new Photos app](https://blogs.windows.com/windowsexperience/2026/10/15/meet-the-new-photos-app-on-windows-11/), [Windows Latest](https://www.windowslatest.com/2026/10/19/windows-11-will-now-have-three-ways-to-view-your-photos-because-apparently-one-wasnt-enough/), [M365 Admin](https://m365admin.handsontek.net/updated-photos-experience-windows-11/).
- **Classic "Import pictures and videos" wizard.** Still in Windows 11, but hidden: right-click a device in This PC, then More options. It can group by Date taken (Day, Month or Year). It only works on a connected device or drive, makes flat folders like `YYYY-MM`, and doesn't help with a folder of photos already on disk. Many users think the feature is gone. Sources: [Microsoft Q&A: import by date taken](https://learn.microsoft.com/en-us/answers/questions/3743126/how-do-i-import-pictures-to-folders-by-date-taken), [Microsoft Q&A: "Import is GONE"](https://learn.microsoft.com/en-us/answers/questions/4098697/how-to-file-imported-photos-in-separate-folders-in), [Pureinfotech](https://pureinfotech.com/import-photos-phone-camera-sd-card-windows-11/).
- **PhotoMove.** Free (unlimited), with Pro at $8.99 one-time. Sorts photos and videos by EXIF date taken, and reads Google Takeout JSON in the free version. In the free version, a file whose name already exists in the destination is skipped, and files with no EXIF date are left where they are. Only Pro can use the file date for those or move them to a folder, and only Pro has templates and file-type filters. No source mentions reading dates from file names. Sources: [mjbpix.com](https://www.mjbpix.com/), [PhotoMove FAQ](https://www.mjbpix.com/faq-photomove/), [Pro options for no-EXIF files](https://www.mjbpix.com/pro-version-options-for-no-exif-data-files/), [Upgrade to Pro](https://www.mjbpix.com/upgrade-to-photomove-2-0-pro/).
- **ExifTool.** Free and very capable, but command-line only: `exiftool -r -d %Y/%m "-Directory<DateTimeOriginal" DIR`. Files without that tag stay put unless you write fallback rules. Videos need other tags such as CreateDate, and Windows batch files need `%%` escaping. Sources: [ExifTool forum](https://exiftool.org/forum/index.php?topic=11887.0), [commandlinefu](https://www.commandlinefu.com/commands/view/2866/move-all-images-in-a-directory-into-a-directory-hierarchy-based-on-year-month-and-day-based-on-exif-information), [childs.be: batch-file escaping](https://www.childs.be/articles/post/getting-exiftools-move-files-to-directories-to-work-properly-as-windows-batch-file).
- **FolioSort.** Commercial, for Windows and Mac. Shows a preview tree, has one-click undo, and falls back to a date in the file name (WhatsApp, camera roll), then to file dates. Also handles videos and Google Takeout. Free for **100 files per batch**; Pro costs €29.99. It is the closest competitor. Sources: [FolioSort: organize by date](https://www.foliosort.app/features/organize-photos-by-date), [FolioSort: ExifTool GUI](https://www.foliosort.app/alternatives/exiftool-gui), [FolioSort's comparison of 9 tools](https://www.foliosort.app/best/photo-organizer-windows) (written by the vendor, so not neutral).
- **Others:** OrganizePhotos (paid, [Microsoft Store](https://apps.microsoft.com/detail/xpfm3zjn0kw1dv?hl=en-US&gl=US)), digiKam, and XnView MP. Open-source scripts: [sortphotos](https://github.com/seancummins/sortphotos) (no filename parsing), [69pmb/Sort-Photos](https://github.com/69pmb/Sort-Photos) (Java, filename fallback), and [negrel/photosort](https://github.com/negrel/photosort) (Rust, only RFC 3339 filename dates).

**What users still complain about**
- **WhatsApp removes EXIF.** Saved images get the date they arrived or the copy date, so they all pile into the month of the backup. The only date left is in the file name (`IMG-20160315-WA0020.jpg`). It is the *received* date, with no time. Separate one-purpose tools exist just to write that date back. Sources: [aaron.cc](https://aaron.cc/adding-the-correct-date-and-time-for-photos-downloaded-from-whatsapp/), [wa-date-restore](https://github.com/CookedApps/wa-date-restore), [holwech blog](https://holwech.github.io/blog/Fixing-WhatsApp-Backup/), [Google Play: DATE for WhatsApp](https://play.google.com/store/apps/details?id=com.appDNA.exiftoolforWA&hl=en), [Overclockers thread](https://forums.overclockers.co.uk/threads/whatsapp-save-photos-based-on-date-taken-not-date-saved.18783430/), [Picorder](https://picorder.app/blogs/whatsapp-photos-wrong-order-in-gallery).
- **File dates are wrong after copying from a phone.** "Date created" becomes the copy date, and "Date modified" isn't reliable either. Videos import with the wrong dates. HEIC "Date taken" can't be edited in Explorer. Sources: [Microsoft Q&A: wrong date importing from iPhone](https://learn.microsoft.com/en-us/answers/questions/3735399/incorrect-date-when-importing-photos-from-iphone-t), [Directory Opus forum](https://resource.dopus.com/t/possible-to-copy-iphone-images-with-correct-creation-date-or-rename-to-correct-creation-date/46634), [takeoutjson.com](https://takeoutjson.com/blog/fix-photo-dates-after-copying-from-phone), [MacRumors thread](https://forums.macrumors.com/threads/some-pictures-and-videos-are-the-wrong-format-when-copying-from-iphone-to-pc.2464470/).
- **Windows 11 import lost date folders.** Users say "Windows 11 will no longer file them in date order" ([Microsoft Q&A](https://learn.microsoft.com/en-us/answers/questions/4098697/how-to-file-imported-photos-in-separate-folders-in), [FM Forums](https://www.fredmiranda.com/forum/topic/1870188/)).

**Can a small new tool add something real?** Yes, but the gap is narrow. Sorting photos that *have* EXIF dates is solved. The unsolved part is a typical phone dump: camera photos mixed with WhatsApp images, screenshots and videos. That needs date fallbacks (EXIF, then video metadata, then the file name, then file dates). Only paid tiers or ExifTool do this well today. Nothing found tells you, file by file, *which* date was used and how much to trust it. FolioSort comes closest, but its free tier stops at 100 files per batch.

## The surviving idea

**Edge in one sentence:** a free, unlimited Windows app that sorts a messy phone dump into Year/Month folders, including WhatsApp images, screenshots and videos with no EXIF date, by trying EXIF, then video metadata, then the date in the file name. It shows a preview that labels where every file's date came from, and it can undo the whole move.

**Riskiest assumption:** that real phone dumps contain enough files with no EXIF date but a usable date in the file name, so that PhotoMove Free and the Windows import wizard visibly get them wrong. If almost every file already has an EXIF or video date, the free tools already work and the edge disappears.

**One simple test that would kill the idea:**
1. Collect 3 to 5 real phone dumps: your own, and a couple from friends (Android camera folder, WhatsApp Images folder, an iPhone import). Work on copies.
2. Run PhotoMove Free on each one, into Year/Month folders.
3. Count the files it left unsorted or put in the wrong month (spot-check the dates against the phone gallery), and how many of those have a date in their file name.
4. **Kill the idea if fewer than about 5% of all files are wrong *and* fixable from their file name.** If so, existing free tools already handle the data people really have.

This takes about an hour and needs no code.

## Product name candidates

Each name was web-searched in quotes on 2026-11-09.

| Name | Result | Status |
|---|---|---|
| **WhenFiled** | No app or product found ([search notes](#name-search-notes)) | Clear |
| **DateDrawer** | Only an internal Flutter UI component in one personal GitHub project ([826-Leoyu PR #63](https://github.com/826-Leoyu/MyPersonalUseApp/pull/63)) | Clear |
| **MonthBox** | One small iOS budget-tracker app ("MonthBox – Simple Monthly Budget Tracker", hosted at geba89.github.io/MonthBox). Not well known and in a different category. Also similar to Chatbooks [Monthbooks](https://chatbooks.com/products/monthbooks) | Usable, but the weakest of the three |

**Rejected while checking:**
- **TidyRoll.** Already used by an iOS photo-cleaner ([App Store](https://apps.apple.com/ch/app/swipe-foto-cleaner-tidyroll/id6760675726)), a macOS iPhone-photo backup tool that sorts by date ([GitHub](https://github.com/nogoodusername/TidyRoll)), and a swipe photo cleaner ([GitHub](https://github.com/gr8monk3ys/tidy-roll)). That's too close.
- **Calendrop.** Already an open-source macOS calendar app ([GitHub](https://github.com/calendrop-app/Calendrop), [site](https://calendrop.lambdalisue.com/)).

### Name search notes
- The "WhenFiled" search returned only results about Microsoft Photos, with no product of that name.
- These checks are web searches, not trademark searches. Search the [USPTO](https://www.uspto.gov/trademarks/search) and [EUIPO](https://euipo.europa.eu/eSearch/) databases before publishing under any of these names.

## Caveats on this research

- Searches came from a US-based web search engine on 2026-11-09. Reddit threads did not come up in results, so complaints come mainly from GitHub issues, Microsoft Q&A, vendor sites and tech press.
- FolioSort's 9-tool comparison is vendor marketing. Where it says nothing about a rival's feature, that feature is unknown, not missing.
- The PowerRename "20-file limit" comes from user reports with no maintainer confirmation.
- One Microsoft Q&A page ([import by date taken](https://learn.microsoft.com/en-us/answers/questions/3743126/how-do-i-import-pictures-to-folders-by-date-taken)) couldn't be fetched directly. Its content here comes from a search-engine summary.
- Whether PhotoMove supports HEIC, and whether it reads dates from file names, was not confirmed either way. The kill test above checks this in practice.
