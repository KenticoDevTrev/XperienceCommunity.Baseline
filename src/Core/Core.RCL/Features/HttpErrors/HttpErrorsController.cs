﻿namespace Core.Features.HttpErrors
{
    public class HttpErrorsController : Controller
    {
        public ActionResult Error(int code)
        {
            return code switch
            {
                404 => Error404(),
                500 => Error500(),
                403 => AccessDenied(),
                _ => View(code),
            };
        }

        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            return View("/Features/HttpErrors/Error404.cshtml");
        }

        public ActionResult Error500()
        {
            Response.StatusCode = 500;
            return View("/Features/HttpErrors/Error500.cshtml");
        }

        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View("/Features/HttpErrors/AccessDenied.cshtml");
        }

        public static string GetAccessDeniedUrl()
        {
            return "error/403";
        }
    }
}