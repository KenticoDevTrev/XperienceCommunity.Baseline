# Kentico Xperience Baseline
This package is part of a large Kentico Xperience Baseline project.  Please refer to the project's website for installation and usage instruction.

In general, you'll want to include references to the following packages

**Foo.Models** => Kentico Agnostic Models + Interfaces
**Foo.Library** => Kentico Agnostic Additional Code
**Foo.RCL** => Kentico Agnostic Components, Features, Etc
**Foo.Library.KX13/XbyK** => Kentico specific implementation of Foo.Model Interfaces
**MVC** (Foo's MVC Site) => Your Website, usually contains both Kentico Agnostic and Specific entities such as Widgets, Templates, etc.

**Foo.Models** => XperienceCommunity.Baseline.Core.Models + optional other XperienceCommunity.Baseline.XXX.Models

**Foo.Library** => Foo.Models + XperienceCommunity.Baseline.Core.Library + optional other XperienceCommunity.Baseline.XXX.Library

**Foo.RCL** => Foo.Library + XperienceCommunity.Baseline.Core.RCL + optional other XperienceCommunity.Baseline.XXX.RCL

**Foo.Library.KX13/XbyK** => Foo.Library + XperienceCommunity.Baseline.Core.Library.KX13/XbyK + optional other XperienceCommunity.Baseline.XXX.Library.KX13/XbyK

**MVC** => Foo.Library.KX13/XbyK + Foo.RCL + XperienceCommunity.Baseline.Core.RCL.KX13/XbyK + optional other XperienceCommunity.Baseline.XXX.RCL.KX13/XbyK 