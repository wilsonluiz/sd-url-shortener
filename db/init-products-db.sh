#!/bin/sh
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL

   CREATE TABLE public.url (
		"Id" varchar NOT NULL,
		"UrlLonga" varchar NULL,
		"UrlCurta" varchar NULL,
		"Criacao" timestamp DEFAULT now() NULL,
		"Ativo" bool DEFAULT true NULL,
		CONSTRAINT url_pk PRIMARY KEY ("Id")
	);

    INSERT INTO public.url ("Id", "UrlLonga", "UrlCurta", "Criacao", "Ativo")
         VALUES ('16aea477-106b-48af-8091-7224bce8a87b', 'https://www1.folha.uol.com.br/mercado/2024/03/inss-corta-aposentadoria-de-martinho-da-vila-por-falta-de-prova-de-vida-saiba-evitar.shtml', '1a67f6a', '2024-03-01 23:29:16.663', true);

	INSERT INTO public.url ("Id", "UrlLonga", "UrlCurta", "Criacao", "Ativo")
		 VALUES ('cfd10f87-1d6b-4a3d-984d-06f7857303a6', 'https://www.treinaweb.com.br/formacoes', '5ef25a3', '2024-03-01 23:36:02.980', true);

	INSERT INTO public.url ("Id", "UrlLonga", "UrlCurta", "Criacao", "Ativo")
		 VALUES ('59399572-24f7-47f8-910e-ba4847cfe2a7', 'https://app.pluralsight.com/library/', 'b4dcecf', '2024-03-02 16:03:37.871', true);

	INSERT INTO public.url ("Id", "UrlLonga", "UrlCurta", "Criacao", "Ativo")
		 VALUES ('c2f25f44-ba81-4c50-ba6a-6b382d47b255', 'https://valor.globo.com/', '40c7b6f', '2024-03-02 16:05:15.629', true);

EOSQL