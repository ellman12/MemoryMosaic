-- Run this file to initialize database for first time use.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; -- https://stackoverflow.com/a/12505220

-- TODO: try indexes!
CREATE TABLE IF NOT EXISTS public.media
(
    path text NOT NULL, -- A short path, inside the library folder, which could be this format: /Unknown/filename.ext, or /2022/10/filename.ext
    date_taken timestamp without time zone,
    date_added timestamp without time zone NOT NULL DEFAULT now(),
    starred boolean NOT NULL DEFAULT false,
    separate boolean NOT NULL DEFAULT false, -- Is this item in a folder?
    uuid uuid NOT NULL DEFAULT uuid_generate_v1(),
    thumbnail text DEFAULT NULL, -- Base64 string representing video thumbnail.
    PRIMARY KEY (path, uuid),
    UNIQUE (path),
    UNIQUE (uuid)
) TABLESPACE pg_default;
ALTER TABLE public.media OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.media_trash
(
    path text NOT NULL,
    date_taken timestamp without time zone,
    date_added timestamp without time zone NOT NULL,
    starred boolean NOT NULL,
    separate boolean NOT NULL,
    uuid uuid NOT NULL,
    thumbnail text,
    date_deleted timestamp without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (path, uuid),
    UNIQUE (path),
    UNIQUE (uuid)
) TABLESPACE pg_default;
ALTER TABLE public.media_trash OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.albums
(
    id serial NOT NULL,
    name text NOT NULL,
    album_cover text DEFAULT NULL REFERENCES media(path) ON DELETE SET NULL, -- References short path in media.
    last_updated timestamp without time zone NOT NULL,
    folder boolean NOT NULL DEFAULT false, -- If this is a folder and thus its contents should remain separate from rest of library.
    PRIMARY KEY (id),
    UNIQUE (name)
) TABLESPACE pg_default;
ALTER TABLE public.albums OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.album_entries
(
    uuid uuid NOT NULL REFERENCES media(uuid), -- TODO: ON DELETE CASCADE?
    album_id integer NOT NULL REFERENCES albums(id), -- TODO: ON DELETE CASCADE?
    date_added_to_album timestamp without time zone NOT NULL,
    PRIMARY KEY (uuid, album_id)
) TABLESPACE pg_default;
ALTER TABLE public.album_entries OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.album_entries_trash
(
    uuid uuid NOT NULL REFERENCES media(uuid), -- TODO: ON DELETE CASCADE?
    album_id integer NOT NULL REFERENCES albums(id), -- TODO: ON DELETE CASCADE?
    date_added_to_album timestamp without time zone NOT NULL,
    PRIMARY KEY (uuid, album_id)
) TABLESPACE pg_default;
ALTER TABLE public.album_entries_trash OWNER to postgres;