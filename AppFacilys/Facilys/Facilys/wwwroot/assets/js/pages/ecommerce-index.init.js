// Vérification de la disponibilité d'ApexCharts
if (typeof ApexCharts === 'undefined') {
    throw new Error('ApexCharts n\'est pas chargé');
}

let monthlyIncomeChart = null;
let paymentChart = null;

export function initMonthlyIncomeChart(data) {
    // Vérification de l'existence de l'élément DOM
    const chartElement = document.querySelector("#monthly_income");
    if (!chartElement) {
        throw new Error('Élément #monthly_income non trouvé');
    }

    // Destruction du chart existant
    if (monthlyIncomeChart) {
        monthlyIncomeChart.destroy();
    }

    const colors = ["#95a0c5", "#95a0c5", "#95a0c5", "#22c55e",
        "#95a0c5", "#95a0c5", "#95a0c5", "#95a0c5",
        "#95a0c5", "#95a0c5", "#95a0c5", "#95a0c5"];

    const options = {
        chart: {
            height: 270,
            type: "bar",
            toolbar: { show: false },
            animations: { enabled: false } // Désactive les animations pour le rendu initial
        },
        colors: colors,
        plotOptions: {
            bar: {
                borderRadius: 6,
                columnWidth: "20%",
                distributed: true
            }
        },
        dataLabels: {
            enabled: false // Désactivé pour éviter les conflits
        },
        series: [{
            name: "Revenu",
            data: data
        }],
        xaxis: {
            categories: ["Jan", "Fev", "Mar", "Avr", "Mai", "Jui","Jul", "Aou", "Sep", "Oct", "Nov", "Dec"],
            labels: {
                style: {
                    colors: "#8997bd",
                    fontSize: "12px"
                }
            }
        },
        yaxis: {
            labels: {
                formatter: function (value) {
                    return value;
                },
                style: {
                    colors: "#8997bd"
                }
            }
        }
    };

    monthlyIncomeChart = new ApexCharts(chartElement, options);
    return monthlyIncomeChart.render();
}

export function updateMonthlyIncomeData(newData) {
    if (monthlyIncomeChart) {
        monthlyIncomeChart.updateSeries([{
            data: newData
        }], true); // Redessine le graphique
    }
}

export function initPaymentChart(data, labels) {
    // Vérification de l'existence de l'élément DOM
    const chartElement = document.querySelector("#payment_chart");
    if (!chartElement) {
        throw new Error('Élément #payment_chart non trouvé');
    }

    // Destruction du chart existant s'il y en a un
    if (paymentChart) {
        paymentChart.destroy();
    }

    const options = {
        chart: {
            height: 280,
            type: "donut",
            toolbar: { show: false },
            animations: { enabled: false } // Désactive les animations pour le rendu initial
        },
        plotOptions: {
            pie: { donut: { size: "80%" } }
        },
        dataLabels: { enabled: false },
        stroke: { show: true, width: 2, colors: ["transparent"] },
        series: data,
        labels: labels, // Utilisation des labels dynamiques
        colors: ["#22c55e", "#08b0e7", "#ffc728", "#ff5c75", "#007bff", "#6f42c1", "#fd7e14", "#343a40"],
        legend: {
            show: true, position: "bottom",
            horizontalAlign: "center", verticalAlign: "middle",
            floating: false, fontSize: "13px",
            fontFamily: "Be Vietnam Pro, sans-serif"
        },
        responsive: [{
            breakpoint: 600,
            options: {
                chart: { height: 240 },
                legend: { show: false }
            }
        }],
        tooltip: {
            y: {
                formatter: function (val) {
                    return `${val} €`; // Affichage du montant en euros
                }
            }
        }
    };

    paymentChart = new ApexCharts(chartElement, options);
    return paymentChart.render();
}
export function updatePaymentChart(newData, newLabels) {
    if (paymentChart) {
        paymentChart.updateOptions({
            series: newData,
            labels: newLabels
        });
    }
}