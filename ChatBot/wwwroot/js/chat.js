function sendMessage() {
    let message = document.getElementById("userInput").value;
    if (!message) return;

    let chatBox = document.getElementById("chatBox");
    chatBox.innerHTML += `<div><b>You:</b> ${message}</div>`;

    fetch('/Chat/ask', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            question: message   // ✅ MUST match ChatRequest.Question
        })
    })
        .then(res => {
            if (!res.ok) {
                throw new Error("API error " + res.status);
            }
            return res.json();
        })
        .then(data => {
            chatBox.innerHTML += `<div><b>Bot:</b> ${data.response}</div>`;
            chatBox.scrollTop = chatBox.scrollHeight;
        })
        .catch(err => {
            console.error(err);
            chatBox.innerHTML += `<div><b>Bot:</b> Error getting response</div>`;
        });

    document.getElementById("userInput").value = "";
}
