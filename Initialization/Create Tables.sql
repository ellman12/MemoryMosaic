-- Run this file to initialize database for first time use.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; -- https://stackoverflow.com/a/12505220

-- TODO: try indexes!
CREATE TABLE IF NOT EXISTS public.library
(
    path text NOT NULL, -- A short path, inside the library folder, which could be this format: /Unknown/filename.ext, or /2022/10/filename.ext
    date_taken timestamp without time zone, -- The date and time, if any, when this item was taken.
    date_added timestamp without time zone NOT NULL DEFAULT now(), -- When was added to library
    starred boolean NOT NULL DEFAULT false,
    separate boolean NOT NULL DEFAULT false, -- Is this item in a folder?
    id uuid NOT NULL DEFAULT uuid_generate_v1(),
    thumbnail text NOT NULL, -- Base64 string representing thumbnail.
    date_deleted timestamp without time zone DEFAULT NULL, -- If this has a value, it's in the Trash.
    description text DEFAULT NULL,
    PRIMARY KEY (path, id),
    UNIQUE (path),
    UNIQUE (id)
) TABLESPACE pg_default;
ALTER TABLE public.library OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.collections
(
    id serial UNIQUE NOT NULL,
    name text UNIQUE NOT NULL,
    cover text REFERENCES library(path) ON UPDATE CASCADE ON DELETE SET NULL DEFAULT NULL,
    last_updated timestamp without time zone NOT NULL, -- The last time this item was renamed, added to/removed from, etc.
    folder boolean NOT NULL DEFAULT false, -- If this is a folder and thus its contents should remain separate from rest of library.
    readonly boolean NOT NULL DEFAULT false, -- If this Collection has been marked as readonly, it cannot: be renamed, have items added/removed, change if it's a folder or not, appear in CollectionSelector, or be deleted.
    PRIMARY KEY (id, name)
) TABLESPACE pg_default;
ALTER TABLE public.collections OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.collection_entries
(
    id uuid NOT NULL REFERENCES library(id) ON DELETE CASCADE,
    collection_id integer NOT NULL REFERENCES collections(id) ON DELETE CASCADE,
    date_added_to_collection timestamp without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (id, collection_id)
) TABLESPACE pg_default;
ALTER TABLE public.collection_entries OWNER to postgres;