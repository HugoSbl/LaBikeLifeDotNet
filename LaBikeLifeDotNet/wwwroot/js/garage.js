// on tape pour filtrer ; les marques sont déjà dans la page, les modèles sont chargés une fois (marque + année)
(function () {
    const make = document.getElementById("make");
    const year = document.getElementById("year");
    const model = document.getElementById("model");
    const modelsList = document.getElementById("models-list");
    if (!make || !year || !model || !modelsList) return;

    const url = model.dataset.modelsUrl;
    const knownMakes = new Set(
        Array.from(document.querySelectorAll("#makes-list option")).map(o => o.value)
    );

    let lastQuery = "";

    function setModelState(placeholder, disabled) {
        model.value = "";
        model.placeholder = placeholder;
        model.disabled = disabled;
    }

    async function loadModels() {
        const mk = make.value.trim();
        const yr = year.value;
        const query = mk + "|" + yr;
        if (query === lastQuery) return; // déjà fait pour cette marque + année, pas la peine de recommencer
        lastQuery = query;

        modelsList.innerHTML = "";
        if (!mk || !yr) {
            setModelState("Choisir d'abord la marque et l'année", true);
            return;
        }

        setModelState("Chargement…", true);
        try {
            const resp = await fetch(`${url}?make=${encodeURIComponent(mk)}&year=${encodeURIComponent(yr)}`);
            if (!resp.ok) throw new Error("HTTP " + resp.status);
            const data = await resp.json();

            modelsList.innerHTML = "";
            if (!Array.isArray(data) || data.length === 0) {
                setModelState("Aucun modèle trouvé pour cette marque/année", true);
                return;
            }
            for (const name of data) {
                const opt = document.createElement("option");
                opt.value = name;
                modelsList.appendChild(opt);
            }
            model.value = "";
            model.placeholder = `Tapez pour filtrer (${data.length} modèles)`;
            model.disabled = false;
        } catch (e) {
            setModelState("Erreur de chargement des modèles", true);
        }
    }

    // on lance la requête seulement si la marque tapée est vraiment une marque de la liste,
    // comme ça on n'appelle pas l'API tant que la saisie n'est pas finie
    function maybeLoad() {
        const mk = make.value.trim();
        if (knownMakes.size > 0 && mk.length > 0 && !knownMakes.has(mk)) return;
        loadModels();
    }

    make.addEventListener("input", maybeLoad);
    make.addEventListener("change", maybeLoad);
    year.addEventListener("change", maybeLoad);
})();
