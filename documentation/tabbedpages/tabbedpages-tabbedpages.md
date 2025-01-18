# Tabbed Pages

This module is a single feature, showing how you can render multiple 'pages' as Tabs under a primary page.

It uses the [PartialWidgetPage](https://github.com/KenticoDevTrev/PartialWidgetPage) System to accomplish this.  The partial widget page allows you to take a Widgetized page, and dynamically render the widgets *within* the render of another page.  

Thus you could have a primary page (IE Tab Parent) which has page builder itself enabled, and then under it have additional Tabs (each with widget zones and rendering), and then render those sub tab pages on the parent page.

An example can be seen on [Convotherm's Company Page](https://www.convotherm.com/Company#), each of those tabs are widget pages.