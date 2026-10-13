/* Dev Login Helper: solo rellena el usuario. La contraseña no va aquí porque wwwroot se publica
   en todos los entornos y este archivo quedaría expuesto en producción. */
(() => {
  const defaultAdminUsername = "owneybeauty";

  const run = () => {
    if (!/\/Account\/Login\/?$/i.test(window.location.pathname)) {
      return;
    }

    const userInput = document.querySelector(
      'input[name$="UserNameOrEmailAddress"], input[id$="UserNameOrEmailAddress"], input[name$="UserName"], input[id$="UserName"]'
    );
    const addHint = (input, text) => {
      if (!input || input.dataset.defaultHintAdded === "true") {
        return;
      }

      const hint = document.createElement("span");
      hint.className = "text-muted small";
      hint.textContent = text;

      const floatingContainer = input.closest(".form-floating.mb-2");
      if (floatingContainer) {
        floatingContainer.appendChild(hint);
        input.dataset.defaultHintAdded = "true";
        return;
      }

      const container =
        input.closest(".input-group") ||
        input.closest(".form-group") ||
        input.closest(".mb-3") ||
        input.closest(".form-floating") ||
        input;

      if (container === input || container.classList.contains("input-group")) {
        container.insertAdjacentElement("afterend", hint);
      } else {
        container.appendChild(hint);
      }
      input.dataset.defaultHintAdded = "true";
    };

    const autoFillDefaults = () => {
      if (userInput && !userInput.value) {
        userInput.value = defaultAdminUsername;
      }
    };

    addHint(userInput, `Default username: ${defaultAdminUsername}`);

    setTimeout(autoFillDefaults, 150);
    if (userInput) {
      userInput.addEventListener("focus", autoFillDefaults, { once: true });
    }
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", run);
  } else {
    run();
  }
})();