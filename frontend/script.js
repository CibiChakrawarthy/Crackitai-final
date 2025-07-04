const shareButton = document.getElementById('share');
const output = document.getElementById('output');

shareButton.addEventListener('click', async () => {
    const stream = await navigator.mediaDevices.getDisplayMedia({ audio: true, video: true });
    const mediaRecorder = new MediaRecorder(stream, { mimeType: 'audio/webm' });
    const chunks = [];

    mediaRecorder.ondataavailable = e => {
        if (e.data.size > 0) chunks.push(e.data);
    };

    mediaRecorder.onstop = async () => {
        const blob = new Blob(chunks, { type: 'audio/webm' });
        const form = new FormData();
        form.append('file', blob, 'audio.webm');
        const response = await fetch('/api/transcribe', { method: 'POST', body: form });
        const { text } = await response.json();

        const chatResp = await fetch('/api/chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ prompt: text })
        });
        const { response: answer } = await chatResp.json();
        output.textContent += `\nQ: ${text}\nA: ${answer}\n`;
    };

    mediaRecorder.start();
    setTimeout(() => mediaRecorder.stop(), 5000); // capture 5 seconds
});
