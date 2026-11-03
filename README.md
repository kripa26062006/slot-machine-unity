# 🎰 Slot Machine (Unity)

A playable slot machine built in Unity for the Game Development Internship assignment.

**▶ Play it in the browser:** `<paste your GitHub Pages / itch.io link here>`

## Game Overview
Three reels spin and stop one after another. The player picks a bet, presses **SPIN**,
and wins when **all reels show the same symbol**. Payout = bet × that symbol's multiplier.
You start with 1000 credits; if you run out, pressing SPIN restarts the game.

## Instructions to Run the WebGL Build
**Online:** open the link above.

**Locally:**
1. Clone the repo.
2. WebGL builds can't be opened by double-clicking `index.html` (browsers block file access).
   Serve the folder instead, e.g. from the repo root:
   `python -m http.server 8000`
3. Open `http://localhost:8000/Build/WebGL/` in your browser.

**From source:** open the project folder in Unity `<your Unity version>`, open `Assets/Scenes/Main.unity`, press Play.

## Bonus Features
- **Wild symbol** that substitutes for any other symbol.
- **Adjustable bet** (10 / 25 / 50 / 100) with a credits balance.
- **Win celebration:** reels pulse and the win amount counts up.
- **Left-to-right staggered reel stops** with a small overshoot "clunk" for game feel.
- **Weighted symbols** so rare symbols pay more (tuned to roughly 95% return-to-player).

## Thought Process / Approach
**Separation of concerns (OOP).** Each class has one job:

| Class | Responsibility |
|---|---|
| `SymbolData` (ScriptableObject) | A symbol's sprite, payout multiplier, weight, wild flag. Designers tweak odds without touching code. |
| `RandomService` | The only place randomness lives. Weighted picking with `System.Random`. |
| `PayoutCalculator` | Pure win/payout rules. No Unity dependencies, so it's easy to test. |
| `Reel` | One reel's visuals and spin animation. |
| `SlotUI` | Labels and buttons; raises events, knows no rules. |
| `SlotMachineController` | Credits and spin flow; connects everything above. |

**Outcome first, animation second.** The result is decided by the RNG *before* the reels move,
then each reel animates onto its pre-decided symbol. The animation is purely cosmetic, so fairness lives in one place.

**Smooth reels.** Each reel is a virtual infinite strip of symbols with only 6 recycled UI cells.
The reel ramps up, spins, then decelerates with a cubic ease-out whose starting speed matches the spin speed
(no visible jerk), overshoots slightly and settles back.

**Symbol table (default tuning)**

| Symbol | Weight | Payout |
|---|---|---|
| Cherry | 30 | x4 |
| Lemon | 25 | x6 |
| Bell | 20 | x10 |
| Bar | 15 | x20 |
| Seven | 10 | x50 |
| Wild | 8 | x100 (only if all three are wilds) |

## Project Structure
```
Assets/
  Scripts/      C# classes
  Prefabs/      Symbol cell, reel
  Animations/   Animation assets
  UI/           UI sprites, fonts
  Sounds/       Audio (optional)
Build/WebGL/    WebGL build
```
