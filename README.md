# Trace — 30-minute time audit

Trace is a single-file web app for logging what you do in 30-minute blocks, so you can see where your day actually goes. Everything lives in `index.html` — no build step, no server, no dependencies. Your data stays on your device in `localStorage`.

## Getting started

1. Open `index.html` in a browser (or host it anywhere over https).
2. On iPhone: open it in Safari, tap **Share → Add to Home Screen**. It launches full-screen like a native app.
3. Set a repeating 30-minute alarm in your Clock app as your nudge to log.

Voice entry and microphone access require the page to be served over **https**.

## Features

### Log
- Log each 30-minute block with a short note of what you did.
- Entries are auto-sorted into buckets (categories) by keyword matching — e.g. "gym" → Fitness, "scrolled instagram" → Scroll / YT.
- A "Just logged" list shows your recent entries for quick review.

### Today
- A timeline of your whole day, block by block, within your configured wake/sleep window.
- Tap any block to fix its bucket or edit the entry.

### Insights
- **Where did the time go?** — breakdown of your day/week by bucket.
- **Paycheck vs your real business** — compares day-job time against time spent building your future (Trading Biz, Charts, AI / Build).
- **Last 7 days** — trend view across the week.
- **Weekly targets** — progress against per-bucket goals.
- **Awareness streak** — how many days in a row you've kept logging.

### More (settings)
- **Voice entry** — dictate a log entry. Uses your own OpenAI API key (stored only on-device) to transcribe with `gpt-4o-mini-transcribe`, falling back to Whisper. Only the recorded audio is sent, only for transcription.
- **Bulk add** — paste a list of lines (optional leading time) to catch up a whole day at once.
- **Day window** — set your usual wake and sleep times to shape the Today timeline.
- **Buckets** — add, remove, or customise categories.
- **Weekly targets** — set a weekly **min** to hit (e.g. Trading Biz) or **max** to stay under (e.g. Scroll / YT) per bucket.
- **Backup & iCloud** — one-tap backup to iCloud Drive (via the share sheet), plus JSON/CSV export and JSON import for restoring on another device. Your API key is never included in backups.

## Default buckets

💼 Paycheck · 💹 Trading Biz · 📈 Charts · 🤖 AI / Build · 🎯 Deep Focus · 📚 Learning · 🧹 Admin · 🏋️ Fitness · 🍽️ Meals · 🚗 Commute · 👪 Family · 🧑‍🏫 Teach Florence · 🤝 People · 📱 Scroll / YT · 🎬 Entertain · 🌙 Rest

## Data & privacy

- All journal data is stored locally in your browser (`localStorage`, key `trace.v1`).
- The optional OpenAI API key is stored on-device only (`trace.ai`) and used to call OpenAI directly from your phone — there is no backend.
- Use **Export JSON** or the iCloud backup regularly; clearing browser data will erase your journal.
