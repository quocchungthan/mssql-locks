const transportState = document.getElementById('transport-state');

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