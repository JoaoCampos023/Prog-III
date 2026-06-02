O QUE JA FOI USADO NO PROJETO:

BACKEND
Tecnologia		Versão	Descrição
.NET			9.0	Framework principal
ASP.NET Core MVC	9.0	Framework web
Entity Framework Core	9.0	ORM para acesso a dados
SQL Server		-	Banco de dados relacional
ASP.NET Core Identity	9.0	Autenticação e autorização

FRONTEND
Tecnologia		Descrição
* Bootstrap 5		Framework CSS responsivo
* jQuery			Manipulação do DOM
* DataTables		Tabelas interativas com paginação
* Font Awesome 6		Ícones vetoriais
* InputMask		Máscaras para campos de formulário
* Google Fonts (Inter)	Fonte personalizada

APIs Integradas
API		Descrição				Endpoint
ViaCEP		Busca de endereços por CEP		https://viacep.com.br/ws/{cep}/json/
DiceBear	Geração de avatares personalizados	https://api.dicebear.com/9.x/{style}/svg

DESIGN PATTERNS IMPLEMENTADOS

* Repository Pattern
* Dependency Injection
* Service Layer
* DTO (Data Transfer Object)
* Facade Pattern
* Strategy Pattern (parcial)
* Builder Pattern (parcial)


OBSERVAÇÕES:

No pc:

* no perfil a data ta sem formatação - ??
* Ao digitar a senha ele pede 6 caracteres, mas ao tentar enviar ele pede que tenha pelo menos 1 digito e uma letra maiscula
* a barra de pesquisa de pessoa não funciona
* O botão de novo cliente é azul e o outros como aeronave e aeroportos são verdes
* Os status nos voos estão com letras minúsculas


Observações - Kalil:

* Acho que a cor de fundo do menu login poderia ser diferente para dar um contraste, ou então deixar a cor principal mais azul como o resto
* Lembre de remover as credenciais no menu de login, se não o professor vai reclamar kkk
* É interessante fazer um readme, nele dá para falar que tem que mudar a string de conexão, também é interessante falar o que precisa para o projeto rodar e a versão dos programas usados, como vc fez aqui em cimaa
* O nome do banco ta em português e as tabelas em ingles
* A coluna AvatarUrl na tabela AspNetUsers não aceita valores nulos, mas o método de criação do usuário administrador está enviando um valor vazio, talvez seja interessante ter uma imagem placeholder para esses casos
* Ele não deixa registrar novos usuários pelo mesmo motivo
* Reports/Revenue está crashando
* Criar dados testes não funcionou para mim
* Menu de novo cliente ainda ta um pouco estranho
* Trocar de avatar ainda não ta funcionando

Observações - João:

* Aba relatorios ainda nao esta 100% funcional, ainda em processo de ajuste.

Ja ajustados:

* metade do código ta em ingles e meta em português
* colocar botão para mostrar senha
* ele não especifica o que tem que ter na senha
* ação rápida de agendar voo não funciona
* no registrar senha o confirmar senha ta com mensagem em field required (em ingles)
* na parte do perfil na direita aparece 2 vezes o email
* os títulos se repetem na aba encima e na pagina em si - nao acho necessario mecher
* Programa crash ao tentar criar um voo
* exportar relatório nao faz nada - Retirado
* as tabelas de aeronaves e aeroportos parecem ter um css um pouco diferente
* seria interessante uma forma de recolher o menu
* não faz sentido poder alterar o estado depois do cep já estar colocado
* acho que a data de nascimento pode ser jogada pra cima, e talvez para ficar par adicionar o sexo
* Aeronaves e aeroportos poderia ter algum tipo de filtra ou busca por nome, como nos outros menus
* Acho que as outras telas devem seguir o padrão do cliente, ela ficou muito boa
