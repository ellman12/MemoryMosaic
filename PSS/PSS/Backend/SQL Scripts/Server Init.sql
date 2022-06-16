-- Run this file to initialize database for first time use.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; -- https://stackoverflow.com/a/12505220

-- TODO: try indexes!
CREATE TABLE public.media
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

CREATE TABLE public.media_trash
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

