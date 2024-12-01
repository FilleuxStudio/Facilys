try {
    new simpleDatatables.DataTable("#datatable_1", {
        searchable: !0,
        fixedHeight: !1
    })
} catch (e) { }
try {
    const b = new simpleDatatables.DataTable("#datatable_2");
    document.querySelector("button.csv").addEventListener("click", () => {
        b.exportCSV({
            type: "csv",
            download: !0,
            lineDelimiter: "\n\n",
            columnDelimiter: ";"
        })
    }), document.querySelector("button.sql").addEventListener("click", () => {
        b.export({
            type: "sql",
            download: !0,
            tableName: "export_table"
        })
    }), document.querySelector("button.txt").addEventListener("click", () => {
        b.export({
            type: "txt",
            download: !0
        })
    }), document.querySelector("button.json").addEventListener("click", () => {
        b.export({
            type: "json",
            download: !0,
            escapeHTML: !0,
            space: 3
        })
    })
} catch (e) { }
try {
    document.addEventListener("DOMContentLoaded", function () {
        var c = document.querySelector("[name='select-all']");
        var n = document.querySelectorAll("[name='check']");

        if (c) {
            c.addEventListener("change", function () {
                var t = c.checked;
                n.forEach(function (e) {
                    e.checked = t;
                });
            });
        }

        n.forEach(function (e) {
            e.addEventListener("click", function () {
                var e = n.length;
                var t = document.querySelectorAll("[name='check']:checked").length;
                if (t <= 0) {
                    c.checked = false;
                    c.indeterminate = false;
                } else if (e === t) {
                    c.checked = true;
                    c.indeterminate = false;
                } else {
                    c.checked = true;
                    c.indeterminate = true;
                }
            });
        });

        var thElements = document.querySelectorAll("table > thead > tr > th");
        if (thElements.length > 0) {
            var firstThButton = thElements[0].querySelector("button:first-child");
            if (firstThButton) {
                firstThButton.classList.remove("datatable-sorter");
            }
        }

        var checkboxAllButton = document.querySelector(".checkbox-all thead tr th:first-child button");
        if (checkboxAllButton) {
            checkboxAllButton.classList.remove("datatable-sorter");
        }
    });
} catch (e) {
    console.error("Une erreur s'est produite :", e);
}
