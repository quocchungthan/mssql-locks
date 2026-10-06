const watchState = document.getElementById('watch-state');
const stateDot = document.getElementById('state-dot');
const statusMessage = document.getElementById('status-message');
const transportState = document.getElementById('transport-state');
const startForm = document.getElementById('start-form');
const stopForm = document.getElementById('stop-form');
const resultsHead = document.getElementById('results-head');
const resultsBody = document.getElementById('results-body');

const setWatchStatus = (status) => {
    watchState.textContent = status.state;
    statusMessage.textContent = status.message;
    stateDot.dataset.state = status.state.toLowerCase();
    startForm.hidden = status.isRunning;
    stopForm.hidden = !status.isRunning;
};

const renderSnapshot = (snapshot) => {
    const headingRow = document.createElement('tr');
    for (const column of snapshot.columns) {
        const heading = document.createElement('th');
        heading.scope = 'col';
        heading.textContent = column;
        headingRow.append(heading);
    }
    resultsHead.replaceChildren(headingRow);

    const rows = snapshot.rows.map((values) => {
        const row = document.createElement('tr');
        for (const value of values) {
            const cell = document.createElement('td');
            cell.textContent = value ?? 'NULL';
            row.append(cell);
        }
        return row;
    });

    if (rows.length === 0) {
        const row = document.createElement('tr');
        row.className = 'empty-row';
        const cell = document.createElement('td');
        cell.colSpan = Math.max(snapshot.columns.length, 1);
        cell.textContent = 'No active grants in this sample.';
        row.append(cell);
        rows.push(row);
    }

    resultsBody.replaceChildren(...rows);
    document.getElementById('waiting-count').textContent = snapshot.waitingCount;
    document.getElementById('granted-count').textContent = snapshot.grantedCount;
    document.getElementById('rows-count').textContent = snapshot.totalRows;
    document.getElementById('sample-time').textContent = new Date(snapshot.capturedAt).toLocaleTimeString();
    document.getElementById('visible-count').textContent = `${snapshot.rows.length} visible${snapshot.isTruncated ? ' of ' + snapshot.totalRows : ''}`;
};

const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/memory-grants')
    .withAutomaticReconnect()
    .build();

connection.on('StatusChanged', setWatchStatus);
connection.on('SnapshotReceived', renderSnapshot);
connection.onreconnecting(() => {
    transportState.textContent = 'LIVE CHANNEL RECONNECTING';
});
connection.onreconnected(() => {
    transportState.textContent = 'LIVE CHANNEL CONNECTED';
});
connection.onclose(() => {
    transportState.textContent = 'LIVE CHANNEL OFFLINE';
});

connection.start()
    .then(() => {
        transportState.textContent = 'LIVE CHANNEL CONNECTED';
    })
    .catch(() => {
        transportState.textContent = 'LIVE CHANNEL OFFLINE';
    });