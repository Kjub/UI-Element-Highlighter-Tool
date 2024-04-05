# Changelog

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
