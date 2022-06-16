-- Run this file to initialize database for first time use.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; -- https://stackoverflow.com/a/12505220

-- TODO: try indexes!
CREATE TABLE IF NOT EXISTS public.media
(
    path text NOT NULL,
    date_taken timestamp without time zone NOT NULL,
    date_added timestamp without time zone NOT NULL DEFAULT now(),
    starred boolean NOT NULL DEFAULT false,
    separate boolean NOT NULL DEFAULT false,
    uuid uuid NOT NULL DEFAULT uuid_generate_v1(),
    thumbnail text DEFAULT NULL,
    PRIMARY KEY (path, uuid),
    CONSTRAINT media_unique_path UNIQUE (path) INCLUDE(path), -- TODO: what are these INCLUDE() things?
    CONSTRAINT media_unique_uuid UNIQUE (uuid) INCLUDE(uuid)
) TABLESPACE pg_default;
ALTER TABLE public.media OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.media_trash
(
    path text NOT NULL,
    date_taken timestamp without time zone NOT NULL,
    date_added timestamp without time zone NOT NULL,
    starred boolean NOT NULL,
    separate boolean NOT NULL,
    uuid uuid NOT NULL,
    thumbnail text,
    date_deleted timestamp without time zone NOT NULL DEFAULT now(),
    PRIMARY KEY (path, uuid),
    CONSTRAINT media_trash_unique_path UNIQUE (path) INCLUDE(path), -- TODO: what are these INCLUDE() things?
    CONSTRAINT media_trash_unique_uuid UNIQUE (uuid) INCLUDE(uuid)
) TABLESPACE pg_default;
ALTER TABLE public.media_trash OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.albums
(
    id serial NOT NULL,
    name text NOT NULL,
    album_cover text DEFAULT NULL references media(path) ON DELETE SET NULL,
    last_updated timestamp without time zone NOT NULL,
    folder boolean NOT NULL DEFAULT false,
    PRIMARY KEY (id, name), -- TODO: might need to be CONSTRAINT albums_pkey PRIMARY KEY (id)... idk why
    CONSTRAINT albums_unique_names UNIQUE (name)
) TABLESPACE pg_default;
ALTER TABLE public.albums OWNER to postgres;