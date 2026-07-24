# VPS Hardening Checklist — forexvps.net (Windows)

Do these in order. Step 4 only after step 3 is confirmed working.

- [ ] **1. Portal 2FA** — enable two-factor auth on your forexvps.net client-area login
      (the portal can reinstall the VPS = master key).
- [ ] **2. Install Tailscale on the VPS** — tailscale.com/download, sign in (free plan),
      note the VPS's `100.x.x.x` address.
- [ ] **3. Install Tailscale on MacBook + iPhone** — same account. **Test RDP to the
      `100.x.x.x` address and confirm it works.**
- [ ] **4. Close public RDP** — Windows Defender Firewall → Inbound Rules →
      Remote Desktop (TCP-In, 3389) → Scope → Remote IP addresses = `100.64.0.0/10` only.
      (Locked out? forexvps.net support/console can restore access.)
- [ ] **5. Account basics** — strong unique Windows password; NLA on (default on modern
      Windows Server); remove/disable unused admin accounts; account lockout policy
      10 attempts / 15 min.
- [ ] **6. Updates on your schedule** — Windows Update active hours + scheduled restart
      **Saturday** (markets closed). Never auto-restart mid-week.
- [ ] **7. cTrader** — logged into the **demo** account for the whole build + soak;
      auto-update OFF if the platform offers the choice (update manually on Saturdays).
- [ ] **8. Secrets note** — Telegram bot token & healthchecks URL will live in cBot
      parameters on this box; treat the VPS as sensitive. Never store the live-account
      master password in a text file.

Done when: RDP unreachable from the public internet, reachable via Tailscale from both
your devices, and a Saturday update schedule is set.
