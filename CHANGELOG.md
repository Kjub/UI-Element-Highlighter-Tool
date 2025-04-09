# Changelog

## [1.0.0] - 09.04.2025

### Changed

-   Refactored code and got it ready to release!

## [0.3.4] - 04.04.2025

### Fixed

-   Fixed logo in settings window

## [0.3.3] - 04.04.2025

### Added

-   Welcome Screen on first run (install)

## [0.3.2] - 04.04.2025

### Changed

-   Changed folder structure of project

## [0.3.1] - 04.04.2025

### Fixed

-   Fixed logo in settings window

## [0.3.0] - 04.04.2025

### Added

-   "Unity 6 context menu option when right clicking in Scene window"
-   "Ability to set shortcut which will run extension's logic on mouse position (default is set to "H" key). Shortcut can be combination of CTRL/SHIFT/ALT + Keybind/Mousebind"

## [0.2.3] - 25.04.2024

### Added

-   "Ability to set if you want to close the selector window upon selecting an element from it"

## [0.2.2] - 05.04.2024

### Updated

-   "Show only active elements" now hides children of disabled objects too

## [0.2.1] - 05.04.2024

### Added

Settings Window

-   "Select Component" option in settings, let's you filter items with this component (default to RectTransform)

### Fixed

-   Error when the extension tried to create GUIStyle outside OnGUI method

## [0.2.0] - 09.03.2024

### Added

Settings Window

-   Added option to enable/disable the extension in settings menu

Selection Window

-   Added information about element if its active or inactive
-   Added option to filter only active elements

### Changed

-   Changed checking of when should extension run the element detection script. (Was on Right Click DOWN, now its on Right Click UP + no dragging)

### Fixed

-   Fixed error when unity wanted to create .meta file but couldn't
-   Fixed error when user deleted an element from hierarchy that was shown in selection window

## [0.1.1] - 04.03.2024

### Fixed

-   Fixed issue when the extension couldn't list found elements after closing prefab view.
