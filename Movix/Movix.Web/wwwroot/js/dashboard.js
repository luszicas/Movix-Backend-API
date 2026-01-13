(function () {
    async function loadData() {
        const res = await fetch('/Admin/Dashboard/Data', { cache: 'no-store' });
        if (!res.ok) return null;
        return await res.json();
    }

    function chart(id, type, labels, data) {
        const el = document.getElementById(id);
        if (!el) return;
        new Chart(el, {
            type,
            data: {
                labels,
                datasets: [{
                    data,
                    borderWidth: 1,
                    backgroundColor: 'rgba(54, 162, 235, 0.5)',
                    borderColor: 'rgba(54, 162, 235, 1)'
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: (type === 'bar' || type === 'line')
                    ? { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    : {}
            }
        });
    }

    document.addEventListener('DOMContentLoaded', async () => {
        const d = await loadData();
        if (!d) return;

        chart('chartGenero', 'bar', d.porGenero.labels, d.porGenero.data);
        chart('chartClass', 'bar', d.porClass.labels, d.porClass.data);
        chart('chartAno', 'line', d.porAno.labels, d.porAno.data);
    });
})();
