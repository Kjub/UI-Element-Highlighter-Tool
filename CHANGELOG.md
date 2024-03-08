# Changelog

## [0.1.1] - 04.03.2024

### Fixed

-   Fixed issue when the extension couldn't list found elements after closing prefab view.

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
