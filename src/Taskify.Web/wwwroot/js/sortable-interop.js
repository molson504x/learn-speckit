export function initializeKanbanBoard(boardElement, dotNetRef) {
    if (!boardElement) {
        return;
    }

    if (typeof boardElement.__taskifyCleanup === "function") {
        boardElement.__taskifyCleanup();
    }

    const disposers = [];
    let draggedTaskId = null;
    const taskCards = boardElement.querySelectorAll("[data-task-id]");
    const columns = boardElement.querySelectorAll("[data-column]");

    for (const taskCard of taskCards) {
        taskCard.setAttribute("draggable", "true");

        const handleDragStart = event => {
            draggedTaskId = taskCard.dataset.taskId ?? null;
            event.dataTransfer?.setData("text/plain", draggedTaskId ?? "");
            taskCard.classList.add("task-card-dragging");
        };

        const handleDragEnd = () => {
            draggedTaskId = null;
            taskCard.classList.remove("task-card-dragging");
        };

        taskCard.addEventListener("dragstart", handleDragStart);
        taskCard.addEventListener("dragend", handleDragEnd);
        disposers.push(() => {
            taskCard.removeEventListener("dragstart", handleDragStart);
            taskCard.removeEventListener("dragend", handleDragEnd);
        });
    }

    for (const column of columns) {
        const handleDragOver = event => {
            event.preventDefault();
            column.classList.add("kanban-column-active");
        };

        const handleDragLeave = () => {
            column.classList.remove("kanban-column-active");
        };

        const handleDrop = event => {
            event.preventDefault();
            column.classList.remove("kanban-column-active");

            const taskId = event.dataTransfer?.getData("text/plain") || draggedTaskId;
            if (!taskId) {
                return;
            }

            dotNetRef.invokeMethodAsync("HandleDropAsync", taskId, column.dataset.column);
        };

        column.addEventListener("dragover", handleDragOver);
        column.addEventListener("dragleave", handleDragLeave);
        column.addEventListener("drop", handleDrop);
        disposers.push(() => {
            column.removeEventListener("dragover", handleDragOver);
            column.removeEventListener("dragleave", handleDragLeave);
            column.removeEventListener("drop", handleDrop);
            column.classList.remove("kanban-column-active");
        });
    }

    boardElement.__taskifyCleanup = () => {
        for (const dispose of disposers) {
            dispose();
        }
    };
}
