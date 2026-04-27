window.qwa = {
    _charts: {},

    createDonutChart: function (id, conformCount, nonConformCount) {
        if (this._charts[id]) {
            this._charts[id].destroy();
            delete this._charts[id];
        }
        const canvas = document.getElementById(id);
        if (!canvas) return;
        this._charts[id] = new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: ['Conformes', 'Non‑conformes'],
                datasets: [{
                    data: [conformCount, nonConformCount],
                    backgroundColor: ['#388E3C', '#C62828'],
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { font: { size: 12 }, padding: 12 }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                var total = ctx.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                var pct = total > 0 ? Math.round(ctx.parsed / total * 100) : 0;
                                return ctx.label + ' : ' + ctx.parsed + ' (' + pct + '%)';
                            }
                        }
                    }
                }
            }
        });
    },

    destroyChart: function (id) {
        if (this._charts[id]) {
            this._charts[id].destroy();
            delete this._charts[id];
        }
    }
};
