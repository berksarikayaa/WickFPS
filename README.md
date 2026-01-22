🎯 Project Overview – Tactical FPS Prototype (Unity)

This project is a systems-focused FPS prototype built in Unity 2022.3 LTS, inspired by John Wick–style gunplay and Resident Evil–like persistent threats.

The core goal of the project is to explore combat readability, resource pressure, and AI-driven encounters, rather than pure arcade shooting.

The result is a tightly controlled FPS sandbox where every bullet matters, enemies react dynamically to being hit, and the player is constantly pushed into risk–reward decisions.

🔫 Core Combat Philosophy

Two-stage enemy takedown system

First body shot downs the enemy

Second shot finishes them

Headshots are lethal and bypass the downed state entirely

Encourages precision and target prioritization, not spray-and-pray gameplay

Limited ammo economy

Magazine + reserve ammo system

Manual reloads

Ammo is a scarce resource, not a given

🧠 Enemy AI Systems

Enemies are not simple “walk-and-shoot” targets. Each enemy uses a layered AI system built on NavMesh + perception logic.

Enemy Capabilities

Line-of-sight based perception

Field of view (FOV)

Distance-based detection

Raycast-based visibility checks (LOS)

Combat-aware movement

Strafing around the player instead of rushing directly

Stop–go behavior to avoid robotic motion

Burst-based gunfire

Enemies fire in controlled bursts

Fire rate and burst size are tunable per enemy type

💥 Hit Reactions & Suppression

Enemies react to being shot in meaningful ways:

Suppression system

Getting hit briefly interrupts enemy fire

Enemies may retreat slightly when damaged

Downed state

Enemies collapse to the ground instead of dying instantly

Creates tension: finish them or manage other threats?

This system creates a more cinematic and readable combat flow, where player actions visibly affect enemy behavior.

📦 Ammo Economy & Risk–Reward Design

Ammo pickups spawn dynamically

Enemies can drop ammo on death

Drop chance and amount are configurable

This creates a risk–reward loop:

Push forward to secure ammo

Or play safe and risk running dry later

The player is constantly making micro-decisions about positioning and aggression.

🎯 Weapon System

Hitscan-based firearms

Visual feedback:

Muzzle flash

Bullet tracers

Impact effects

Fully decoupled weapon logic for:

Player weapons

Enemy weapons

This allows easy expansion into new weapon types later.

🧭 Navigation & Stability

Enemies are safely spawned and snapped onto the NavMesh

AI gracefully handles cases where movement is unavailable

Extensive debugging tools were built during development to ensure:

Correct perception logic

Reliable line-of-sight detection

Stable AI state transitions

🧪 Current State

✔ Player shooting & ammo management
✔ Enemy perception, movement, and shooting
✔ Downed / kill logic
✔ Ammo drops and resource pressure
✔ Stable NavMesh-based AI

The prototype is currently fully playable as a combat sandbox.

🚀 Planned Features

The following systems are planned for future iterations:

Boss enemy (Nemesis-style)

Constantly pursues the player

High pressure, limited suppression

Unique close-range attacks

Accuracy scaling

Enemy accuracy changes based on movement and distance

Advanced cover behavior

Enemies actively seek cover instead of open strafing

Player feedback improvements

Camera shake on damage

Enhanced hit reactions

Stronger visual/audio cues

Additional enemy archetypes

Aggressive rushers

Defensive shooters

Elite enemies with better aim and resistance

🛠️ Tech Stack

Engine: Unity 2022.3 LTS

Language: C#

AI: NavMeshAgent + custom perception logic

Architecture Focus: Modular, system-driven, expandable

🧠 Design Intent

This project is intentionally built as a learning-focused, systems-first FPS.
The emphasis is on clarity, control, and player decision-making, not raw spectacle.

It serves as a strong foundation for:

A full single-player FPS

A vertical slice demo

Or a combat-focused portfolio piece
