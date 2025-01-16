# Error Pages

The Baseline comes with an [HttpErrorsController](../../src/Core/Core.RCL/Features/HttpErrors/HttpErrorsController.cs) for 404, 403, 500, and any other error types.

## Configure

You can use this by configuring the route:

```csharp
 app.MapControllerRoute(
    name: "error",
    pattern: "error/{code}",
       defaults: new { controller = "HttpErrors", action = "Error" }
);
```

## Customize Views

You will want to customize the views, simply use the [standard customization point](../general/customization-points.md) and place your own views in these locations:

- /Features/HttpErrors/Error404.cshtml
- /Features/HttpErrors/AccessDenied.cshtml
- /Features/HttpErrors/Error500.cshtml
- /Features/HttpErrors/Error.cshtml (Model is an int for the error code)