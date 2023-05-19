# Kentico Xperience Baseline
This package is part of a large Kentico Xperience Baseline project.  Please refer to the project's website for installation and usage instruction.

In general, you'll want to include references to the following packages

* **Foo.Models** (Kentico Agnostic Models + Interfaces)
  * XperienceCommunity.Baseline.Core.Models
  * Optional other XperienceCommunity.Baseline.XXX.Models
* **Foo.Library** (Kentico Agnostic Additional Code)
  * Foo.Models
  * XperienceCommunity.Baseline.Core.Library
  * Optional other XperienceCommunity.Baseline.XXX.Library
* **Foo.RCL** (Kentico Agnostic Components, Features, Etc)
  * Foo.Library
  * XperienceCommunity.Baseline.Core.RCL
  * Optional other XperienceCommunity.Baseline.XXX.RCL
* **Foo.Library.KX13/XbyK** (Kentico specific implementation of Foo.Model Interfaces)
  * Foo.Library
  * XperienceCommunity.Baseline.Core.Library.KX13/XbyK
  * Optional other XperienceCommunity.Baseline.XXX.Library.KX13/XbyK
* **MVC** (Your Website, usually contains both Kentico Agnostic and Specific entities such as Widgets, Templates, etc.)
  * Foo.Library.KX13/XbyK
  * Foo.RCL
  * XperienceCommunity.Baseline.Core.RCL.KX13/XbyK
  * Optional other XperienceCommunity.Baseline.XXX.RCL.KX13/XbyK 