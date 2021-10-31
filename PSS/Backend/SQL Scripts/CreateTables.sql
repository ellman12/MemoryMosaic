-- Used to create all the tables in PostgreSQL
CREATE TABLE IF NOT EXISTS public.album_entries
(
    path text COLLATE pg_catalog."default" NOT NULL,
    album_id integer NOT NULL,
    date_added_to_album timestamp without time zone NOT NULL,
    CONSTRAINT album_entries_pkey PRIMARY KEY (path, album_id)
    )

    TABLESPACE pg_default;

ALTER TABLE public.album_entries
    OWNER to postgres;


CREATE TABLE IF NOT EXISTS public.album_entries_trash
(
    path text COLLATE pg_catalog."default" NOT NULL,
    album_id integer NOT NULL,
    date_added_to_album timestamp without time zone NOT NULL,
    CONSTRAINT album_entries_trash_pkey PRIMARY KEY (path, album_id)
    )

    TABLESPACE pg_default;

ALTER TABLE public.album_entries_trash
    OWNER to postgres;


CREATE TABLE IF NOT EXISTS public.albums
(
    -- id serial NOT NULL,
    id integer NOT NULL DEFAULT nextval('albums_id_seq'::regclass),
    name text COLLATE pg_catalog."default" NOT NULL,
    album_cover text COLLATE pg_catalog."default",
    CONSTRAINT albums_pkey PRIMARY KEY (id),
    CONSTRAINT albums_name_key UNIQUE (name)
)

TABLESPACE pg_default;

ALTER TABLE public.albums
    OWNER to postgres;


CREATE TABLE IF NOT EXISTS public.media
(
    path text COLLATE pg_catalog."default" NOT NULL,
    date_added timestamp without time zone NOT NULL,
    date_taken timestamp without time zone NOT NULL,
    uuid uuid NOT NULL DEFAULT uuid_generate_v1(),
    CONSTRAINT media_pkey PRIMARY KEY (path, uuid),
    CONSTRAINT albums_name_key UNIQUE (path)
)

TABLESPACE pg_default;

ALTER TABLE public.media
    OWNER to postgres;


CREATE TABLE IF NOT EXISTS public.media_trash
(
    path text COLLATE pg_catalog."default" NOT NULL,
    date_added timestamp without time zone NOT NULL,
    date_taken timestamp without time zone NOT NULL,
    uuid uuid NOT NULL,
    CONSTRAINT media_trash_pkey PRIMARY KEY (path, uuid)
    )

    TABLESPACE pg_default;

ALTER TABLE public.media_trash
    OWNER to postgres;