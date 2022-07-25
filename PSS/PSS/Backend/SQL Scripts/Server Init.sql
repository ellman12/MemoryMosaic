-- Run this file to initialize database for first time use.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; -- https://stackoverflow.com/a/12505220

-- TODO: try indexes!
CREATE TABLE IF NOT EXISTS public.media
(
    path text NOT NULL, -- A short path, inside the library folder, which could be this format: /Unknown/filename.ext, or /2022/10/filename.ext
    date_taken timestamp without time zone, -- The date and time, if any, when this item was taken.
    date_added timestamp without time zone NOT NULL DEFAULT now(), -- When was added to library
    starred boolean NOT NULL DEFAULT false,
    separate boolean NOT NULL DEFAULT false, -- Is this item in a folder?
    uuid uuid NOT NULL DEFAULT uuid_generate_v1(),
    thumbnail text DEFAULT NULL, -- Base64 string representing video thumbnail.
    date_deleted timestamp without time zone DEFAULT NULL, -- If this has a value, it's not in the Trash.
    PRIMARY KEY (path, uuid),
    UNIQUE (path),
    UNIQUE (uuid)
) TABLESPACE pg_default;
ALTER TABLE public.media OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.collections
(
    id serial NOT NULL,
    name text NOT NULL,
    collection_cover text DEFAULT NULL REFERENCES media(path) ON DELETE SET NULL, -- References short path in media. If the cover is deleted from media, remove cover from any collections.
    last_updated timestamp without time zone NOT NULL, -- The last time this item was renamed, added to/removed from, etc.
    folder boolean NOT NULL DEFAULT false, -- If this is a folder and thus its contents should remain separate from rest of library.
    PRIMARY KEY (id),
    UNIQUE (name)
) TABLESPACE pg_default;
ALTER TABLE public.collections OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.collection_entries
(
    uuid uuid NOT NULL REFERENCES media(uuid) ON DELETE CASCADE,
    collection_id integer NOT NULL REFERENCES collections(id) ON DELETE CASCADE,
    date_added_to_collection timestamp without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (uuid, collection_id)
) TABLESPACE pg_default;
ALTER TABLE public.collection_entries OWNER to postgres;