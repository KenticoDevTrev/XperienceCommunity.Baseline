/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./typescript/Helper/Helpers/SiteMethods.ts":
/*!**************************************************!*\
  !*** ./typescript/Helper/Helpers/SiteMethods.ts ***!
  \**************************************************/
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.SiteMethods = void 0;
class SiteMethods {
}
exports.SiteMethods = SiteMethods;


/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
var __webpack_exports__ = {};
// This entry need to be wrapped in an IIFE because it need to be isolated against other modules in the chunk.
(() => {
var exports = __webpack_exports__;
/*!************************************!*\
  !*** ./typescript/Helper/index.ts ***!
  \************************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
const SiteMethods_1 = __webpack_require__(/*! ./Helpers/SiteMethods */ "./typescript/Helper/Helpers/SiteMethods.ts");
// Can access your methods with window.SiteMethods.theMethodName()
window.SiteMethods = SiteMethods_1.SiteMethods;

})();

/******/ })()
;
//# sourceMappingURL=helpers.js.map