# Crackitai-final

This repository holds a prototype for a live interview assistant. The backend is built with ASP.NET Core while the frontend uses plain JavaScript so it can evolve to any framework in the future.

## Setup

1. Install the .NET SDK 8.0 or newer.
2. `cd backend`
3. Run the API with `dotnet run`. The API serves the frontend from the `frontend` folder.
4. Navigate to `http://localhost:5000` in your browser.

The front end captures audio from a shared browser tab, sends it to the backend, and displays the ChatGPT response.

To capture audio from a meeting tab the browser will ask to share your screen or tab when you press **Share Meeting Tab**. After five seconds the audio is sent to the API for transcription and the response from ChatGPT is shown below the button. This is a minimal starting point and can be expanded with WebSockets and a richer UI later.

