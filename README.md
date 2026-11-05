# 🎰 Slot Machine (Unity)

Hi, I'm Kripa, a 3rd-year CSE student at SMVIT. Game development is what I want to do long term, and this is my take on the Game Development Internship assignment. Before this I built **Capybara Quest**, a platformer in C++ and SFML, so Unity and C# were a fresh challenge for me.

**▶ Play it in the browser:** https://kripa26062006.github.io/slot-machine-unity/Build/webGL/

## Game Overview
Three reels spin and stop one after another, left to right. You pick a bet, press **SPIN**, and win when **all three reels show the same symbol**. Payout = bet × that symbol's multiplier.
You start with 1000 credits. If you run out, pressing SPIN restarts the game.

## Instructions to Run the WebGL Build
**Online:** open the link above.

**Locally:**
1. Clone the repo.
2. Browsers block WebGL builds opened by double-clicking `index.html`, so serve the folder instead. From the repo root run: `python -m http.server 8000`
3. Open `http://localhost:8000/Build/webGL/` in your browser.

**From source:** open the project folder in Unity `6000.3.26f1`, open `Assets/Scenes/SampleScene.unity`, and press Play.

## Bonus Features
- **Adjustable bet** (10 / 25 / 50 / 100) with a live credits balance.
- **Win celebration:** the reels pulse and the win amount counts up.
- **Staggered reel stops** with a small overshoot "clunk", so the stop feels mechanical instead of abrupt.
- **Weighted symbols:** rarer symbols pay more (roughly 89% return-to-player).

## Thought Process / Approach
**Keep each class to one job.** I split the game so I could change one part without breaking the others:

| Class | Responsibility |
|---|---|
| `SymbolData` (ScriptableObject) | A symbol's sprite, payout multiplier and weight. I can tune the odds without touching code. |
| `RandomService` | The only place randomness lives (weighted picking). |
| `PayoutCalculator` | Pure win and payout rules. |
| `Reel` | One reel's visuals and spin animation. |
| `SlotUI` | Labels and buttons. It raises events and knows no rules. |
| `SlotMachineController` | Credits and spin flow; connects everything above. |

**Decide the result first, animate second.** The RNG picks the outcome before the reels move, and each reel then animates onto its pre-decided symbol. The animation is only visual, so the fairness of the game lives in one place.

**Smooth reels without wasted work.** Each reel is a virtual endless strip of symbols, with only 6 UI cells that get recycled. The reel ramps up, spins, slows down with an ease-out curve that starts at the spin speed (so there's no visible jerk), overshoots slightly, and settles back.

**Symbol table**

| Symbol | Weight | Payout |
|---|---|---|
| Cherry | 40 | x5 |
| Bell | 30 | x10 |
| BAR | 20 | x25 |
| Seven | 10 | x100 |

## What I'd Add With More Time
- Sound effects for spins, stops and wins.
- Custom button art and a bigger win animation.
- A proper payline display and more symbols.

## Project Structure
- `Assets/Scripts/` has the C# classes.
- `Assets/Prefabs/` has the symbol cell.
- `Assets/Data/` has the symbol ScriptableObjects.
- `Build/webGL/` has the WebGL build.

## Credits
Slot machine art and symbols come from the asset pack provided with the assignment.