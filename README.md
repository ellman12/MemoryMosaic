# Photos Storage Server

## About
Work in progress improved clone of Google Photos developed for my personal use on the local network.<br>
The plan is to have a server on my local network running this Blazor Server app and I can access the server from any authorized machine on the local network. The PostgresSQL database is for storing things like:
* What photos/videos are in the library
* What albums there are and what items are in them
* What items are in the trash
* Etc.

Since starting this project I've started to realize just how slow, bloated, and occasionally glitchy Google Photos is. Lots of slow, annoying animations and ads throughout the app, as well as some other buggy behavior sometimes. One goal I had/have with this is to make it as fast, simple, and streamlined as possible: no annoying animations that waste time, no weird behavior, etc.

### Features
* Store your photos and videos on a local server as opposed to giving Google—or another cloud provider—access to all your media.
* Group items together in albums
  * Good use cases include making albums for vacation pictures, family/pet pictures, or other things.
* Put items in folders to keep them separate from your main library.
  * Like Photos' archiving but can have as many as you want as opposed to Photos' single 'Archive' option.
  * Best used for things like documents or school notes you don't want mixed in your main library.
* Easily backup your photos and videos, as well as your library (the database) to wherever you want. Also makes it easy to move your library to another server.

## How to Use and Setup
TODO
