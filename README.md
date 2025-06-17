# FilmLibraryBot

A Telegram bot for movie and TV series lovers to manage their watchlists, keep track of watched content, rate titles, and pick something random to watch.

## Features

- **Personal Library**: Add movies or TV series you want to watch to your collection
- **Library Management**: View your library and manage entries with ease
- **Tracking**: Mark titles as watched, remove from your list, or rate them
- **Rating System**: Rate watched content on your personal scale
- **Watch History**: View watched titles with your ratings and links to TMDB
- **Random Picker**: Let the bot choose something to watch from your library (filtered by movie, series, or both)
- **TMDB Integration**: Search and find accurate movie/series information

## Bot Commands

| Command | Description |
|---------|-------------|
| `/addmovie` | Add a movie or TV series to your library |
| `/showlibrary` | Show your current library and manage it |
| `/random` | Pick a random title to watch (by type or any) |
| `/watchedlibrary` | View your watched list with ratings |

## Technology Stack

- **Platform**: Telegram Bot API
- **Framework**: .NET 8.0
- **Database**: PostgreSQL
- **API**: The Movie Database (TMDB)

## Prerequisites

- .NET 8.0
- PostgreSQL database
- TMDB API key (for movie/series info)
- Telegram Bot API token

## Installation

1. **Register your bot with BotFather on Telegram to get a token**

2. **Clone this repository**:
   ```bash
   git clone https://github.com/Ha1fmoon/FilmBot.git
   cd FilmBot
   ```

3. **Create and fill out the environment file**:
   ```bash
   cp .env.template .env
   # Edit .env with your credentials
   ```

4. **Build and run the bot**:
   ```bash
   dotnet build
   dotnet run
   ```

## Configuration

Set the following environment variables (example in `.env` or your deployment config):

```
TELEGRAM_TOKEN=your_telegram_bot_token
TMDB_API_KEY=your_tmdb_api_key
CONNECTION_STRING=Host=localhost;Port=5432;Database=filmbot;Username=your_username;Password=your_password
BOT_LANGUAGE=en
TMDB_LANGUAGE=en-US
```

## Database Structure

The bot uses PostgreSQL with the following main tables:
- **movies** - stores movie and series information
- **users** - tracks bot users
- **watchlist** - maps users to their "to watch" items
- **watched** - tracks watched content with user ratings

## Database Schema

```sql
CREATE TABLE movies (
  id BIGINT PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  year INTEGER,
  rating DECIMAL(3, 1),
  overview TEXT,
  poster_path VARCHAR(255),
  page_path VARCHAR(255),
  media_type INTEGER NOT NULL
);

CREATE TABLE users (
  id BIGINT PRIMARY KEY,
  username VARCHAR(255) NOT NULL
);

CREATE TABLE watchlist (
  movie_id BIGINT REFERENCES movies(id),
  user_id BIGINT REFERENCES users(id),
  PRIMARY KEY (movie_id, user_id)
);

CREATE TABLE watched (
  movie_id BIGINT REFERENCES movies(id),
  user_id BIGINT REFERENCES users(id),
  user_rating INTEGER CHECK (user_rating >= 0 AND user_rating <= 10),
  PRIMARY KEY (movie_id, user_id)
);

CREATE INDEX idx_watchlist_user_id ON watchlist(user_id);
CREATE INDEX idx_watched_user_id ON watched(user_id);
```

## Supported Languages

- English (en)
- Russian (ru)

Language can be configured through the `BOT_LANGUAGE` environment variable.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
