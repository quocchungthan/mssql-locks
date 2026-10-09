const transportState = document.getElementById('transport-state');
const chartNamespace = 'http://www.w3.org/2000/svg';
let memoryGrantsHistory = [];

const renderTable = (head, body, columns, rows, emptyMessage) => {
    const headerRow = document.createElement('tr');
    for (const column of columns) {
        const heading = document.createElement('th');
        heading.scope = 'col';
        heading.textContent = column;
        headerRow.append(heading);
    }
    head.replaceChildren(headerRow);

    const renderedRows = rows.map((values) => {
        const row = document.createElement('tr');
        for (const value of values) {
            const cell = document.createElement('td');
            cell.textContent = value ?? 'NULL';
            row.append(cell);
        }
        return row;
    });

    if (renderedRows.length === 0) {
        const row = document.createElement('tr');
        row.className = 'empty-row';
        const cell = document.createElement('td');
        cell.colSpan = Math.max(columns.length, 1);
        cell.textContent = emptyMessage;
        row.append(cell);
        renderedRows.push(row);
    }

    body.replaceChildren(...renderedRows);
};

const setStatus = (prefix, status) => {
    document.getElementById(`${prefix}-state`).textContent = status.state;
    document.getElementById(`${prefix}-state`).dataset.state = status.state.toLowerCase();
    document.getElementById(`${prefix}-message`).textContent = status.message;
    document.getElementById(`${prefix}-start-form`).hidden = status.isRunning;
    document.getElementById(`${prefix}-stop-form`).hidden = !status.isRunning;
    if (prefix === 'memory-grants') {
        document.getElementById('clear-memory-grants-history-form').hidden = status.isRunning;
    }
};

const addSvgElement = (svg, name, attributes, text) => {
    const element = document.createElementNS(chartNamespace, name);
    for (const [key, value] of Object.entries(attributes)) {
        element.setAttribute(key, value);
    }
    if (text !== undefined) {
        element.textContent = text;
    }
    svg.append(element);
    return element;
};

const renderHistoryChart = (svgId, historyPoints, series, unit) => {
    const svg = document.getElementById(svgId);
    svg.replaceChildren();
    if (historyPoints.length === 0) {
        addSvgElement(svg, 'text', { x: 450, y: 140, 'text-anchor': 'middle', class: 'chart-empty' }, 'No saved samples yet. Start the watch to collect history.');
        return;
    }

    const points = historyPoints.length > 240
        ? historyPoints.filter((point, index) => index % Math.ceil(historyPoints.length / 240) === 0 || index === historyPoints.length - 1)
        : historyPoints;
    const plot = { left: 76, right: 884, top: 22, bottom: 205 };
    const width = plot.right - plot.left;
    const height = plot.bottom - plot.top;
    const values = series.flatMap((item) => points.map((point) => item.getValue(point)));
    const maximum = Math.max(...values, 1);

    for (let tick = 0; tick <= 4; tick++) {
        const fraction = tick / 4;
        const y = plot.bottom - fraction * height;
        const value = maximum * fraction;
        addSvgElement(svg, 'line', { x1: plot.left, y1: y, x2: plot.right, y2: y, class: 'chart-grid' });
        addSvgElement(svg, 'text', { x: plot.left - 9, y: y + 4, 'text-anchor': 'end', class: 'chart-label' }, unit === 'GiB' ? value.toFixed(maximum < 10 ? 1 : 0) : Math.round(value).toLocaleString());
    }

    addSvgElement(svg, 'line', { x1: plot.left, y1: plot.bottom, x2: plot.right, y2: plot.bottom, class: 'chart-axis' });
    const labelStride = Math.max(1, Math.ceil(points.length / 5));
    for (let index = 0; index < points.length; index++) {
        if (index % labelStride !== 0 && index !== points.length - 1) {
            continue;
        }
        const x = points.length === 1 ? plot.left + width / 2 : plot.left + (index / (points.length - 1)) * width;
        const label = new Date(points[index].capturedAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        addSvgElement(svg, 'text', { x, y: 226, 'text-anchor': 'middle', class: 'chart-label' }, label);
    }

    for (const item of series) {
        const coordinates = points.map((point, index) => {
            const x = points.length === 1 ? plot.left + width / 2 : plot.left + (index / (points.length - 1)) * width;
            const y = plot.bottom - (item.getValue(point) / maximum) * height;
            return `${x},${y}`;
        });
        addSvgElement(svg, 'polyline', {
            points: coordinates.join(' '),
            fill: 'none',
            stroke: item.color,
            'stroke-width': 3,
            'stroke-linejoin': 'round',
            'stroke-linecap': 'round',
        });
    }
};

const renderHistoryReport = () => {
    const orderedPoints = [...memoryGrantsHistory].sort((left, right) =>
        new Date(left.capturedAt).getTime() - new Date(right.capturedAt).getTime());
    const count = orderedPoints.length;
    const range = document.getElementById('history-range');
    if (count === 0) {
        range.textContent = 'No saved samples';
    } else {
        const first = new Date(orderedPoints[0].capturedAt).toLocaleString();
        const last = new Date(orderedPoints[count - 1].capturedAt).toLocaleString();
        range.textContent = `${count.toLocaleString()} samples · ${first} to ${last}`;
    }

    const kibToGib = (value) => value / 1048576;
    renderHistoryChart('semaphore-memory-chart', orderedPoints, [
        { color: '#176b51', getValue: (point) => kibToGib(point.availableMemoryKb) },
        { color: '#c16b2c', getValue: (point) => kibToGib(point.targetMemoryKb) },
    ], 'GiB');
    renderHistoryChart('requested-memory-chart', orderedPoints, [
        { color: '#c16b2c', getValue: (point) => kibToGib(point.waitingRequestedMemoryKb) },
        { color: '#398d91', getValue: (point) => kibToGib(point.grantedRequestedMemoryKb) },
    ], 'GiB');
    renderHistoryChart('maximum-wait-chart', orderedPoints, [
        { color: '#b34855', getValue: (point) => point.maximumWaitTimeMs },
    ], 'ms');
};

const clearHistoryView = () => {
    memoryGrantsHistory = [];
    renderHistoryReport();
    document.getElementById('waiting-count').textContent = '0';
    document.getElementById('granted-count').textContent = '0';
    document.getElementById('rows-count').textContent = '0';
    document.getElementById('memory-grants-time').textContent = 'No sample yet';
    renderTable(
        document.getElementById('results-head'),
        document.getElementById('results-body'),
        [],
        [],
        'Start the watch to capture memory grants.');
};

const loadMemoryGrantsHistory = async () => {
    try {
        const response = await fetch('/Home/MemoryGrantsHistory', { cache: 'no-store' });
        if (!response.ok) {
            throw new Error('History could not be loaded.');
        }
        memoryGrantsHistory = await response.json();
        renderHistoryReport();
    } catch {
        document.getElementById('history-range').textContent = 'History could not be loaded.';
    }
};

const renderMemoryGrants = (snapshot) => {
    renderTable(
        document.getElementById('results-head'),
        document.getElementById('results-body'),
        snapshot.columns,
        snapshot.rows,
        'No active grants in this sample.');

    document.getElementById('waiting-count').textContent = snapshot.waitingCount;
    document.getElementById('granted-count').textContent = snapshot.grantedCount;
    document.getElementById('rows-count').textContent = snapshot.totalRows;
    document.getElementById('memory-grants-time').textContent = new Date(snapshot.capturedAt).toLocaleTimeString();
    memoryGrantsHistory.push(snapshot.historyPoint);
    renderHistoryReport();
};

const renderCapacity = (snapshot) => {
    renderTable(
        document.getElementById('capacity-head'),
        document.getElementById('capacity-body'),
        snapshot.columns,
        [snapshot.values],
        'No capacity sample received.');
    document.getElementById('capacity-time').textContent = new Date(snapshot.capturedAt).toLocaleTimeString();
};

const makeConnection = (url, onStatus, onSnapshot) => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .build();

    connection.on('StatusChanged', onStatus);
    connection.on('SnapshotReceived', onSnapshot);
    connection.onreconnecting(() => {
        transportState.textContent = 'LIVE CHANNEL RECONNECTING';
    });
    connection.onreconnected(() => {
        transportState.textContent = 'LIVE CHANNEL CONNECTED';
    });
    connection.onclose(() => {
        transportState.textContent = 'LIVE CHANNEL OFFLINE';
    });

    return connection;
};

const memoryGrantsConnection = makeConnection(
    '/hubs/memory-grants',
    (status) => setStatus('memory-grants', status),
    renderMemoryGrants);
memoryGrantsConnection.on('HistoryCleared', clearHistoryView);
const capacityConnection = makeConnection(
    '/hubs/capacity',
    (status) => setStatus('capacity', status),
    renderCapacity);

Promise.all([memoryGrantsConnection.start(), capacityConnection.start()])
    .then(() => {
        transportState.textContent = 'LIVE CHANNEL CONNECTED';
    })
    .catch(() => {
        transportState.textContent = 'LIVE CHANNEL OFFLINE';
    });

const categorySelect = document.getElementById('report-category');
const reportSelect = document.getElementById('report-name');

const filterReports = () => {
    const visibleReports = [];
    for (const option of reportSelect.options) {
        const visible = option.dataset.category === categorySelect.value;
        option.hidden = !visible;
        option.disabled = !visible;
        if (visible) {
            visibleReports.push(option);
        }
    }

    if (visibleReports.length > 0 && reportSelect.selectedOptions[0]?.dataset.category !== categorySelect.value) {
        reportSelect.value = visibleReports[0].value;
    }
};

categorySelect.addEventListener('change', filterReports);
filterReports();
loadMemoryGrantsHistory();

document.getElementById('clear-memory-grants-history-form').addEventListener('submit', (event) => {
    if (!window.confirm('Clear all saved memory-grants history? This cannot be undone.')) {
        event.preventDefault();
    }
});