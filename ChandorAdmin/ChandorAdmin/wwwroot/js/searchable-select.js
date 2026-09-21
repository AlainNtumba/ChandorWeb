let observer;
let scheduled = false;
let current;
const widgets = new Map();

const normalize = value => (value ?? "")
    .toString()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLocaleLowerCase();

function closeCurrent() {
    if (!current) return;
    current.root.classList.remove("is-open");
    current.button.setAttribute("aria-expanded", "false");
    current.panel.hidden = true;
    current = null;
}

function positionPanel(widget) {
    const rect = widget.root.getBoundingClientRect();
    const availableBelow = window.innerHeight - rect.bottom - 12;
    const openAbove = availableBelow < 240 && rect.top > availableBelow;
    widget.panel.style.left = `${Math.max(8, rect.left)}px`;
    widget.panel.style.width = `${Math.max(220, rect.width)}px`;
    widget.panel.style.maxWidth = `${Math.max(220, window.innerWidth - 16)}px`;
    widget.panel.style.top = openAbove ? "auto" : `${rect.bottom + 2}px`;
    widget.panel.style.bottom = openAbove ? `${window.innerHeight - rect.top + 2}px` : "auto";
}

function optionText(option) {
    return (option.textContent || option.label || "").trim();
}

function refresh(widget) {
    const { select, button, value, list, search, empty } = widget;
    const options = Array.from(select.options);
    const selected = options.find(option => option.selected) || options[0];
    value.textContent = selected ? optionText(selected) : (select.getAttribute("placeholder") || "Sélectionnez");
    value.classList.toggle("is-placeholder", !selected || selected.value === "" || selected.value === "00000000-0000-0000-0000-000000000000");
    button.disabled = select.disabled;
    button.setAttribute("aria-disabled", select.disabled ? "true" : "false");
    list.replaceChildren();

    const query = normalize(search.value);
    let visibleCount = 0;
    options.forEach((option, index) => {
        if (option.hidden) return;
        const text = optionText(option);
        if (query && !normalize(text).includes(query)) return;
        visibleCount++;
        const item = document.createElement("button");
        item.type = "button";
        item.className = "searchable-select__option";
        item.textContent = text;
        item.dataset.value = option.value;
        item.disabled = option.disabled;
        item.setAttribute("role", "option");
        item.setAttribute("aria-selected", option.selected ? "true" : "false");
        item.tabIndex = option.selected ? 0 : -1;
        item.addEventListener("click", () => {
            select.selectedIndex = index;
            select.dispatchEvent(new Event("input", { bubbles: true }));
            select.dispatchEvent(new Event("change", { bubbles: true }));
            search.value = "";
            closeCurrent();
            refresh(widget);
            button.focus();
        });
        list.appendChild(item);
    });
    empty.hidden = visibleCount !== 0;
}

function open(widget) {
    if (widget.select.disabled) return;
    if (current === widget) {
        closeCurrent();
        return;
    }
    closeCurrent();
    current = widget;
    widget.root.classList.add("is-open");
    widget.button.setAttribute("aria-expanded", "true");
    widget.panel.hidden = false;
    widget.search.value = "";
    refresh(widget);
    positionPanel(widget);
    requestAnimationFrame(() => widget.search.focus());
}

function enhance(select) {
    if (widgets.has(select) || select.multiple || select.closest(".searchable-select")) return;

    const root = document.createElement("div");
    root.className = "searchable-select";
    const button = document.createElement("button");
    button.type = "button";
    button.className = "searchable-select__trigger";
    button.setAttribute("aria-haspopup", "listbox");
    button.setAttribute("aria-expanded", "false");
    const value = document.createElement("span");
    value.className = "searchable-select__value";
    const caret = document.createElement("span");
    caret.className = "searchable-select__caret";
    caret.setAttribute("aria-hidden", "true");
    button.append(value, caret);
    root.appendChild(button);
    select.insertAdjacentElement("afterend", root);
    select.classList.add("searchable-select__native");

    const panel = document.createElement("div");
    panel.className = "searchable-select__panel";
    panel.hidden = true;
    const searchBox = document.createElement("div");
    searchBox.className = "searchable-select__search-box";
    const search = document.createElement("input");
    search.type = "search";
    search.className = "searchable-select__search";
    search.placeholder = "Rechercher…";
    search.autocomplete = "off";
    search.setAttribute("aria-label", "Rechercher dans la liste");
    const clear = document.createElement("button");
    clear.type = "button";
    clear.className = "searchable-select__clear";
    clear.innerHTML = "&times;";
    clear.setAttribute("aria-label", "Effacer la recherche");
    searchBox.append(search, clear);
    const list = document.createElement("div");
    list.className = "searchable-select__options";
    list.setAttribute("role", "listbox");
    const empty = document.createElement("div");
    empty.className = "searchable-select__empty";
    empty.textContent = "Aucun résultat";
    panel.append(searchBox, list, empty);
    document.body.appendChild(panel);

    const widget = { select, root, button, value, panel, search, clear, list, empty };
    widgets.set(select, widget);
    button.addEventListener("click", () => open(widget));
    search.addEventListener("input", () => refresh(widget));
    clear.addEventListener("click", () => {
        search.value = "";
        refresh(widget);
        search.focus();
    });
    select.addEventListener("change", () => refresh(widget));
    select.addEventListener("input", () => refresh(widget));
    search.addEventListener("keydown", event => {
        if (event.key === "Escape") {
            closeCurrent();
            button.focus();
            return;
        }
        if (event.key !== "ArrowDown") return;
        event.preventDefault();
        list.querySelector(".searchable-select__option:not(:disabled)")?.focus();
    });
    list.addEventListener("keydown", event => {
        if (!["ArrowDown", "ArrowUp", "Escape"].includes(event.key)) return;
        event.preventDefault();
        if (event.key === "Escape") {
            closeCurrent();
            button.focus();
            return;
        }
        const items = Array.from(list.querySelectorAll(".searchable-select__option:not(:disabled)"));
        const index = items.indexOf(document.activeElement);
        const next = event.key === "ArrowDown" ? Math.min(items.length - 1, index + 1) : Math.max(0, index - 1);
        items[next]?.focus();
    });
    refresh(widget);
}

function synchronize() {
    scheduled = false;
    document.querySelectorAll("select:not([multiple])").forEach(enhance);
    widgets.forEach((widget, select) => {
        if (!select.isConnected) {
            widget.panel.remove();
            widget.root.remove();
            widgets.delete(select);
            if (current === widget) current = null;
        } else {
            refresh(widget);
        }
    });
}

function scheduleSynchronize() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(synchronize);
}

function onMutations(mutations) {
    const containsExternalChange = mutations.some(mutation => {
        const element = mutation.target.nodeType === Node.ELEMENT_NODE
            ? mutation.target
            : mutation.target.parentElement;
        return !element?.closest?.(".searchable-select, .searchable-select__panel");
    });
    if (containsExternalChange) scheduleSynchronize();
}

export function initialize() {
    if (observer) return;
    synchronize();
    observer = new MutationObserver(onMutations);
    observer.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ["disabled", "value", "selected"] });
    document.addEventListener("pointerdown", onDocumentPointerDown, true);
    window.addEventListener("resize", onViewportChange);
    window.addEventListener("scroll", onViewportChange, true);
}

function onDocumentPointerDown(event) {
    if (!current || current.root.contains(event.target) || current.panel.contains(event.target)) return;
    closeCurrent();
}

function onViewportChange() {
    if (current) positionPanel(current);
}

export function dispose() {
    observer?.disconnect();
    observer = null;
    document.removeEventListener("pointerdown", onDocumentPointerDown, true);
    window.removeEventListener("resize", onViewportChange);
    window.removeEventListener("scroll", onViewportChange, true);
    widgets.forEach(widget => widget.panel.remove());
    widgets.clear();
    current = null;
}
