-- Run this file to initialize database for first time use.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; -- https://stackoverflow.com/a/12505220

CREATE TABLE IF NOT EXISTS public.library
(
	path text UNIQUE NOT NULL, -- A short path, inside the library folder, which could be this format: Unknown/filename.ext, or 2023/10/filename.ext
	id uuid UNIQUE NOT NULL DEFAULT uuid_generate_v1(),
	date_taken timestamp without time zone,
	date_added timestamp without time zone NOT NULL DEFAULT now(),
	separate boolean NOT NULL DEFAULT false, -- Is this item in a folder?
	starred boolean NOT NULL DEFAULT false,
	description text DEFAULT NULL,
	date_deleted timestamp without time zone DEFAULT NULL, -- If this has a value, it's in the Trash.
	thumbnail text NOT NULL, -- Compressed base64 string representing thumbnail.
	PRIMARY KEY (path, id)
) TABLESPACE pg_default;
ALTER TABLE public.library OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.collections
(
	id serial UNIQUE NOT NULL,
	name text UNIQUE NOT NULL,
	cover text REFERENCES library(path) ON UPDATE CASCADE ON DELETE SET NULL DEFAULT NULL,
	folder boolean NOT NULL DEFAULT false, -- If this is a folder and thus its contents should remain separate from rest of library.
	readonly boolean NOT NULL DEFAULT false, -- If this Collection has been marked as readonly, it cannot: be renamed, have items added/removed, change if it's a folder or not, appear in CollectionSelector, or be deleted.
	last_modified timestamp without time zone NOT NULL, -- The last time this item was renamed, added to/removed from, etc.
	PRIMARY KEY (id, name)
) TABLESPACE pg_default;
ALTER TABLE public.collections OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.collection_entries
(
    collection_id integer NOT NULL REFERENCES collections(id) ON DELETE CASCADE,
    item_id uuid NOT NULL REFERENCES library(id) ON DELETE CASCADE,
    date_added_to_collection timestamp without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (collection_id, item_id)
) TABLESPACE pg_default;
ALTER TABLE public.collection_entries OWNER to postgres;