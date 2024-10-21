window.kentico = window.kentico || {};

window.kentico.updatableFormHelper = (function () {

    // Duration for which user must not type anything in order for the form to be submitted.
    var KEY_UP_DEBOUNCE_DURATION = 800;

    /**
     * Registers event listeners and updates the form upon changes of the form data.
     * @param {object} config Configuration object.
     * @param {string} config.formId ID of the form element.
     * @param {string} config.targetAttributeName Data attribute of element that is used to be replaced by HTML received from the server.
     * @param {string} config.unobservedAttributeName Data attribute which marks an input as not being observed for changes.
     */
    function registerEventListeners(config) {
        if (!config || !config.formId || !config.targetAttributeName || !config.unobservedAttributeName) {
            throw new Error("Invalid configuration passed.");
        }

        var writeableTypes = ["email", "number", "password", "search", "tel", "text", "time"];

        var observedForm = document.getElementById(config.formId);
        if (!(observedForm && observedForm.getAttribute(config.targetAttributeName))) {
            return;
        }

        for (i = 0; i < observedForm.length; i++) {
            var observedFormElement = observedForm.elements[i];
            var handleElement = !observedFormElement.hasAttribute(config.unobservedAttributeName) &&
                observedFormElement.type !== "submit";

            if (handleElement) {
                var isWriteableElement = (observedFormElement.tagName === "INPUT" && writeableTypes.indexOf(observedFormElement.type) !== -1) || observedFormElement.tagName === "TEXTAREA";

                if (isWriteableElement) {
                    observedFormElement.previousValue = observedFormElement.value;

                    observedFormElement.addEventListener("keyup", debounce(function (e) {
                        setTimeout(function () {
                            if (!observedForm.updating && e.target.previousValue !== e.target.value) {
                                observedForm.keyupUpdate = true;
                                const changedFieldName = extractFieldName(e.target);
                                updateForm(observedForm, e.target, changedFieldName);
                            }
                        }, 0);
                    }, KEY_UP_DEBOUNCE_DURATION));

                    observedFormElement.addEventListener("blur", function (e) {
                        setTimeout(function () {
                            if (!observedForm.updating && e.target.previousValue !== e.target.value) {
                                const changedFieldName = extractFieldName(e.target)
                                updateForm(observedForm, e.relatedTarget, changedFieldName);
                            }
                        }, 0);
                    });
                }

                observedFormElement.addEventListener("change", function (e) {
                    setTimeout(function () {
                        if (!observedForm.updating) {
                            const changedFieldName = extractFieldName(e.target);
                            updateForm(observedForm, null, changedFieldName);
                        }
                    }, 0);
                });
            }
        }
    }

    /**
     * Updates the form markup.
     * @param {HTMLElement} form Element of the form to update.
     * @param {Element} nextFocusElement Element which shout get focus after update.
     */
    function updateForm(form, nextFocusElement, changedFieldName) {
        if (!form) {
            return;
        }

        // If form is not updatable then do nothing 
        var elementIdSelector = form.getAttribute("data-ktc-ajax-update");
        if (!elementIdSelector) {
            return;
        }

        form.querySelectorAll("input[type='submit']").forEach((item) => {
            item.setAttribute("onclick", "return false;");
        });

        form.updating = true;

        var formData = new FormData(form);
        formData.append("kentico_update_form", "true");

        if (changedFieldName) {
            formData.append("kentico_changed_form_field_name", changedFieldName);
        }

        var focus = nextFocusElement || document.activeElement;

        var onResponse = function (event) {
            if (!event.target.response.data) {
                var selectionStart = selectionEnd = null;
                if (focus && (focus.type === "text" || focus.type === "search" || focus.type === "password" || focus.type === "tel" || focus.type === "url")) {
                    selectionStart = focus.selectionStart;
                    selectionEnd = focus.selectionEnd;
                }

                var currentScrollPosition = window.scrollY;
                var element = document.getElementById(elementIdSelector);

                if (useJQuery()) {
                    $(element).replaceWith(event.target.responseText);
                } else {
                    renderMarkup(event.target.responseText, element);
                }
                window.scrollTo(0, currentScrollPosition);

                if (focus.id) {
                    var newInput = document.getElementById(focus.id);
                    if (newInput) {
                        newInput.focus();
                        setCaretPosition(newInput, selectionStart, selectionEnd);
                    }
                }
            }
        };

        createRequest(form, formData, onResponse);
    }

    function submitForm(event) {
        event.preventDefault();
        var form = event.target;
        var formData = new FormData(form);

        var onResponse = function (event) {
            var contentType = event.target.getResponseHeader("Content-Type");

            if (contentType.indexOf("application/json") === -1) {
                var currentScrollPosition = window.scrollY;
                var replaceTargetId = form.getAttribute("data-ktc-ajax-update");

                var element = document.getElementById(replaceTargetId);
              
                if (useJQuery()) {
                    $(element).replaceWith(event.target.response);
                } else {
                    renderMarkup(event.target.response, element)
                }

                window.scrollTo(0, currentScrollPosition);
            } else {
                var json = JSON.parse(event.target.response);

                location.href = json.redirectTo;
            }
        };

        createRequest(form, formData, onResponse);
    }

    const renderMarkup = (markup, targetElement) => {
        targetElement.innerHTML = markup;
        const scripts = targetElement.querySelectorAll("script");
        Array.prototype.forEach.call(scripts, (scriptElement) => {
            const parent = scriptElement.parentNode;
            const temp = document.createElement("script");
            [...scriptElement.attributes].forEach((attr) => {
                temp.setAttribute(attr.name, attr.value);
            });

            temp.innerHTML = scriptElement.innerHTML;
            parent.replaceChild(temp, scriptElement);
            scriptElement.remove();
        });
    };

    function createRequest(form, formData, onResponse) {
        var xhr = new XMLHttpRequest();

        xhr.addEventListener("load", onResponse);

        xhr.open("POST", form.action);
        xhr.send(formData);
    }

    /**
     * Sets the caret position.
     * @param {HTMLInputElement} input Input element in which the caret position should be set.
     * @param {number} selectionStart Selection start position.
     * @param {number} selectionEnd Selection end position.
     */
    function setCaretPosition(input, selectionStart, selectionEnd) {
        if (selectionStart === null && selectionEnd === null) {
            return;
        }

        if (input.setSelectionRange) {
            input.setSelectionRange(selectionStart, selectionEnd);
        }
    }

    function debounce(func, wait, immediate) {
        var timeout;

        return function () {
            var context = this,
                args = arguments;

            var later = function () {
                timeout = null;

                if (!immediate) {
                    func.apply(context, args);
                }
            };

            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait || 200);

            if (callNow) {
                func.apply(context, args);
            }
        };
    }

    function extractFieldName(targetElement) {
        const fullName = targetElement.getAttribute("name") ?? "";
        const splitName = fullName.split(".");
        const secondToLast = splitName.length - 2;
        return secondToLast >= 0 ? splitName[secondToLast] : "";
    }

    function useJQuery() {
        return window.kentico.hasOwnProperty("builder") ? window.kentico.builder.useJQuery : false;
    }

    return {
        registerEventListeners: registerEventListeners,
        updateForm: updateForm,
        submitForm: submitForm
    };
}());