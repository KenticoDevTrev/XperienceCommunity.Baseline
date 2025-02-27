﻿namespace Core.Repositories
{
    public interface IPageContextRepository
    {
        /// <summary>
        /// If the current page is in edit mode
        /// </summary>
        /// <returns></returns>
        Task<bool> IsEditModeAsync();

        /// <summary>
        /// If the current page is in preview mode
        /// </summary>
        /// <returns></returns>
        Task<bool> IsPreviewModeAsync();

        /// <summary>
        /// Gets the current Page Identity if the request was derived from routing
        /// </summary>
        /// <returns></returns>
        Task<Result<PageIdentity>> GetCurrentPageAsync();

        /// <summary>
        /// Gets the PageIdentity information based on the given identifier
        /// </summary>
        /// <param name="identity">The Tree Identity (you can use string/int/guid .ToTreeIdentity() extension)</param>
        /// <returns>The Page Identity</returns>
        Task<Result<PageIdentity>> GetPageAsync(TreeIdentity identity);

        /// <summary>
        /// Gets the PageIdentity information based on the given identifier (for cultured requests)
        /// </summary>
        /// <param name="identity">The Tree Culture Identity.  (you can use string/int/guid .TreeCultureIdentity() extension)</param>
        /// <returns>The Page Identity </returns>
        Task<Result<PageIdentity>> GetPageAsync(TreeCultureIdentity identity);

        /// <summary>
        /// Gets the current Page Identity if the request was derived from routing
        /// </summary>
        /// <typeparam name="T">The Type of PageIdentity (Content Type or Reusable Schema Interface), will return the PageIdentity of that type IF the retrieved page is of that type or inherits the given Interface.</typeparam>
        /// <returns></returns>
        Task<Result<PageIdentity<T>>> GetCurrentPageAsync<T>();

        /// <summary>
        /// Gets the PageIdentity information based on the given identifier
        /// </summary>
        /// <param name="identity">The Tree Identity (you can use string/int/guid .ToTreeIdentity() extension)</param>
        /// <typeparam name="T">The Type of PageIdentity (Content Type or Reusable Schema Interface), will return the PageIdentity of that type IF the retrieved page is of that type or inherits the given Interface.</typeparam>
        /// <returns>The Page Identity</returns>
        Task<Result<PageIdentity<T>>> GetPageAsync<T>(TreeIdentity identity);

        /// <summary>
        /// Gets the PageIdentity information based on the given identifier (for cultured requests)
        /// </summary>
        /// <param name="identity">The Tree Culture Identity.  (you can use string/int/guid .TreeCultureIdentity() extension)</param>
        /// <typeparam name="T">The Type of PageIdentity (Content Type or Reusable Schema Interface), will return the PageIdentity of that type IF the retrieved page is of that type or inherits the given Interface.</typeparam>
        /// <returns>The Page Identity </returns>
        Task< Result<PageIdentity<T>>> GetPageAsync<T>(TreeCultureIdentity identity);


        /// <summary>
        /// Gets the PageIdentity information based on the given identifier
        /// </summary>
        /// <param name="identity">The Node Identity (you can use string/int/guid .ToNodeIdentity() extension)</param>
        /// <returns>The Page Identity</returns>
        [Obsolete("Use GetPage(TreeIdentity)")]
        Task<Result<PageIdentity>> GetPageAsync(NodeIdentity identity);

        /// <summary>
        /// Gets the PageIdentity information based on the given identifier
        /// </summary>
        /// <param name="identity">The Document Identity (you can use string/int/guid .ToDocumentIdentity() extension)</param>
        /// <returns>The Page Identity </returns>
        [Obsolete("Use GetPage(TreeCultureIdentity)")]
        Task<Result<PageIdentity>> GetPageAsync(DocumentIdentity identity);
    }
}
