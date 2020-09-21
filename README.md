# PluralSight - Build An Async API With ASP.Net Core
(Curso da PluralSight sobre API assíncrona, desenvolvida em ASP.Net Core)

Esta API é o resultado final do curso. Existe algumas variações que devem serem citadas, no meu caso, não usei o SQL Server como provedor e sim o PostgreSQL,
por isso os packages necessários são ligeiramente diferentes.

Fiz o desenvolvimento todo usando o VSCode, por isso precisei fazer uma adaptação com relação ao HTTPClientFactory, para adicionar um handler para ignorar problemas
de validação com certificado SSL da API BookCovers.API.
