using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movix.Domain.Entities;
using Movix.Infrastructure.Entities;

namespace Movix.Infrastructure.Persistence.Seed
{
    /// <summary>
    /// Aplica migrations e insere dados iniciais de forma idempotente.
    /// Ordem: Migrate -> TiposUsuario -> Roles -> Usuários padrão -> Catálogos (Gênero, Classificação, Filmes)
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task EnsureSeededAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var ctx = sp.GetRequiredService<AppDbContext>();
            await ctx.Database.MigrateAsync();

            // =======================
            // Tipos de usuário
            // =======================
            if (!await ctx.TiposUsuario.AnyAsync())
            {
                ctx.TiposUsuario.AddRange(
                    new TipoUsuario { Descricao = "Administrador" },
                    new TipoUsuario { Descricao = "Gerente" },
                    new TipoUsuario { Descricao = "Outros" }
                );
                await ctx.SaveChangesAsync();
            }

            var tipoAdminId = await ctx.TiposUsuario.Where(t => t.Descricao == "Administrador").Select(t => t.Id).FirstAsync();
            var tipoGerenteId = await ctx.TiposUsuario.Where(t => t.Descricao == "Gerente").Select(t => t.Id).FirstAsync();
            var tipoOutrosId = await ctx.TiposUsuario.Where(t => t.Descricao == "Outros").Select(t => t.Id).FirstAsync();

            // =======================
            // Roles (Identity)
            // =======================
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            async Task EnsureRoleAsync(string roleName)
            {
                if (!await roleMgr.RoleExistsAsync(roleName))
                {
                    var create = await roleMgr.CreateAsync(new IdentityRole<Guid>(roleName));
                    if (!create.Succeeded)
                    {
                        var msg = string.Join("; ", create.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Falha ao criar role '{roleName}': {msg}");
                    }
                }
            }

            await EnsureRoleAsync("Admin");
            await EnsureRoleAsync("Gerente");
            await EnsureRoleAsync("Outros");

            // =======================
            // Usuários padrão
            // =======================
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

            async Task<ApplicationUser> EnsureUserAsync(
                string userName, string email, string password, string role, int tipoUsuarioId)
            {
                var user = await userMgr.FindByEmailAsync(email);
                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = userName,
                        Email = email,
                        EmailConfirmed = true,
                        IsActive = true,
                        TipoUsuarioId = tipoUsuarioId,
                        CreatedAt = DateTime.UtcNow
                    };

                    var create = await userMgr.CreateAsync(user, password);
                    if (!create.Succeeded)
                    {
                        var msg = string.Join("; ", create.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Falha ao criar usuário '{email}': {msg}");
                    }
                }
                else
                {
                    if (!user.IsActive || user.TipoUsuarioId != tipoUsuarioId)
                    {
                        user.IsActive = true;
                        user.TipoUsuarioId = tipoUsuarioId;
                        user.UpdatedAt = DateTime.UtcNow;
                        await ctx.SaveChangesAsync();
                    }
                }

                if (!await userMgr.IsInRoleAsync(user, role))
                {
                    var add = await userMgr.AddToRoleAsync(user, role);
                    if (!add.Succeeded)
                    {
                        var msg = string.Join("; ", add.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Falha ao atribuir role '{role}' ao usuário '{email}': {msg}");
                    }
                }

                return user;
            }

            await EnsureUserAsync("admin", "admin@movix.com", "Admin@123", "Admin", tipoAdminId);
            await EnsureUserAsync("gerente", "gerente@movix.com", "Gerente@123", "Gerente", tipoGerenteId);
            await EnsureUserAsync("usuario", "usuario@movix.com", "Usuario@123", "Outros", tipoOutrosId);

            // =======================
            // Catálogos: Gêneros e Classificações
            // =======================
            if (!await ctx.Generos.AnyAsync())
            {
                ctx.Generos.AddRange(
                    new Genero { Nome = "Lançamentos" },
                    new Genero { Nome = "Terror" },
                    new Genero { Nome = "Comédia" },
                    new Genero { Nome = "Animação" },
                    new Genero { Nome = "Romance" },
                    new Genero { Nome = "Ação" }
                   
                );
                await ctx.SaveChangesAsync();
            }

            if (!await ctx.Classificacoes.AnyAsync())
            {
                ctx.Classificacoes.AddRange(
                    new Classificacao { Nome = "Livre", Descricao = "Livre Para todas as idades" },
                    new Classificacao { Nome = "12", Descricao = "Não recomendado para menores de 12" },
                    new Classificacao { Nome = "14", Descricao = "Não recomendado para menores de 14" },
                    new Classificacao { Nome = "16", Descricao = "Não recomendado para menores de 16" },
                    new Classificacao { Nome = "18", Descricao = "Proibido para menores de 18" }
                );
                await ctx.SaveChangesAsync();
            }

            // =======================
            // Filmes — somente se não houver nenhum (evita "ressurgimento" do seed)
            // =======================

            async Task EnsureFilmAsync(
                string titulo, int ano, string generoNome, string classifNome, string imagemUrl, string? sinopse, string? urlTrailer)
            {
                if (!await ctx.Filmes.AnyAsync(f => f.Titulo == titulo))
                {
                    var generoId = await ctx.Generos.Where(g => g.Nome == generoNome).Select(g => g.Id).FirstAsync();
                    var classId = await ctx.Classificacoes.Where(c => c.Nome == classifNome).Select(c => c.Id).FirstAsync();
                    ctx.Filmes.Add(new Filme
                    {
                        Titulo = titulo,
                        Ano = ano,
                        GeneroId = generoId,
                        ClassificacaoId = classId,
                        ImagemCapaUrl = imagemUrl,
                        Sinopse = sinopse,
                        UrlTrailer = urlTrailer,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (!await ctx.Filmes.AnyAsync())
            {
                /*Lançamentos*/
                await EnsureFilmAsync(
                    "Missão Pet", 2025, "Lançamentos", "Livre",
                    "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcRR31N8VYDygS4VyoaDekwZBld7HyTWnIVmxdyKk7VmDH2bpWyb",
                    "Uma equipe de bandidos animais embarca em um golpe de rotina e se veem envolvidos em um assalto a trem. Cabe a Falcon, um guaxinim ladrão de pouca monta, e Rex, um cão policial justo, salvar os animais neste trem em alta velocidade.",
                    "https://youtu.be/AXmoo8xSTow?si=1TkXVBEaqy7Z8qF7");

                await EnsureFilmAsync(
                    "O Rei da Feira", 2025, "Lançamentos", "16",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTkggOnR_DIkm8QlcEfbBAIJhyOmGxZxPbLZZhNfGWF1jiIc5oZ",
                    "Um feirante assassinado misteriosamente volta para ajudar a solucionar seu caso.",
                    "https://youtu.be/L0NFuws5a3A?si=dftgy_f0wv--uqfh");

                await EnsureFilmAsync(
                    "Faça Ela Voltar", 2025, "Lançamentos", "18",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ5w8cqDQucQPnPpqRdYB4zQOGxlxzmJvYJ077ZlBRE_Ylxpm85",
                    "Um irmão e uma irmã testemunham um ritual aterrorizante na casa isolada de sua nova mãe adotiva.",
                    "https://youtu.be/Rw0CYv3Topg?si=w4ItHoFNXckQ9kCN");

                await EnsureFilmAsync(
                    "A Mulher No Jardim", 2025, "Lançamentos", "16",
                    "https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcQTEfrasQHyL_q6XzX3E55CI6LGzAFSO5r90z9-zbvLfYxl1oM_",
                    "Ramona fica paralisada pela dor após a morte de seu marido, deixando-a sozinha para cuidar de seus dois filhos. Sua tristeza logo se transforma em medo quando uma mulher espectral vestida de preto aparece em seu jardim da frente.",
                    "https://youtu.be/ZMRTDkW0q6w?si=4mxUr94j8iRSk6R9");

                await EnsureFilmAsync(
                    "Um Natal Ex-pecial", 2025, "Lançamentos", "16",
                    "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcS_bsqoAGXU4RBQO3XlLhQthg2KNr5Yh3zLk1MokX8O7OeRb8ob",
                    "Kate e Everett querem de presente de Natal um divórcio amigável e um último fim de ano em família. Mas novos amores e antigos sentimentos podem atrapalhar tudo.",
                    "https://youtu.be/4CA01Ptow1Y?si=Y2HMa0BwOLdyXnZw");

                await EnsureFilmAsync(
                    "Frankenstein", 2025, "Lançamentos", "18",
                    "https://admin.cnnbrasil.com.br/wp-content/uploads/sites/12/2025/05/frankenstein.jpg?w=1200&h=1200&crop=1",
                    "Um cientista brilhante, mas egoísta, traz uma criatura monstruosa à vida em um experimento ousado que, em última análise, leva à ruína tanto do criador quanto de sua trágica criação.",
                    "https://youtu.be/IZ4qobQAto8?si=MtPpPURcNqFqtbZz");

                /*Terror*/
                await EnsureFilmAsync(
                    "It: A Coisa", 2017, "Terror", "16",
                    "https://picsum.photos/seed/terrorLanc1/400/600",
                    "Um grupo de crianças se une para investigar o misterioso desaparecimento de vários jovens em sua cidade. Eles descobrem que o culpado é Pennywise, um palhaço cruel que se alimenta de seus medos e cuja violência teve origem há vários séculos.",
                    "https://youtu.be/CwXOrWvPBPk");

                await EnsureFilmAsync(
                  "A Bruxa", 2015, "Terror", "16",
                  "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcRpTIDMdsWEStI5yAkPYtJcfloTOkYVCeCrD0ahQeHSnyYjBHcn",
                  "Em uma fazenda no século 17, uma histeria religiosa toma conta de uma família que acusa a filha mais velha pelo desaparecimento do seu irmão ainda bebê.",
                  "https://youtu.be/FE-u6RznkGQ?si=c8wy9zthGgEt3Wwd");

                await EnsureFilmAsync(
                  "A Freira", 2018, "Terror", "14",
                  "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQi9gpXlV_tLug_NWT1kv6SMKJ7EHMteCPl7eUAMmUXBcri2ldJ",
                  "Presa em um convento na Romênia, uma freira comete suicídio. Para investigar o caso, o Vaticano envia um padre assombrado e uma noviça prestes a se tornar freira. Arriscando suas vidas, a fé e até suas almas, os dois descobrem um segredo profano e se confrontam com uma força do mal que toma a forma de uma freira demoníaca e transforma o convento em um campo de batalha.",
                  "https://youtu.be/4V44ew-laC4?si=sd5nNdsy497zWnCT");

                await EnsureFilmAsync(
                  "Aterrorizante 3", 2024, "Terror", "18",
                  "https://br.web.img3.acsta.net/img/f4/89/f4896a73d4a7285ec4620431bbc4569b.jpg",
                  "Após sobreviver ao massacre de Halloween do Palhaço Art, Sienna e seu irmão lutam para reconstruir suas vidas despedaçadas. No entanto, justo quando pensam que estão seguros, Art retorna, determinado a transformar sua alegria natalina em um pesadelo.",
                  "https://youtu.be/2r-rBilynzo?si=3kV82zGc0L_wOLvB");

                await EnsureFilmAsync(
                  "A Maldição da Casa Winchester", 2018, "Terror", "14",
                  "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSjNniAEhiUvbBiUK5PZbdMw-mDbxlRzQVdS6nd0qX3jAzjvilM",
                  "Sarah Winchester é herdeira de uma empresa de armas de fogo e acredita ser assombrada por almas que foram mortas pelo rifle criado por sua família, os Winchester. Após as repentinas mortes do marido e filho, ela decide construir uma mansão para afastar os espíritos. Quando o psiquiatra Eric Price parte para avaliar o estado psicológico de Sarah, ele percebe que talvez a obsessão dela não seja tão insana assim.",
                  "https://youtu.be/VtJC3h-xIJQ?si=YwjlYLH3Pg1n4nLw");

                await EnsureFilmAsync(
                  "Fale Comigo", 2022, "Terror", "16",
                  "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS_EoX8vv7rux4yd1HPBR5q82SZGer0Qy33Xp1D2q-N_0vFxy7m",
                  "Um grupo de amigos descobre uma mão embalsamada que lhes permite conjurar espíritos. Viciados na emoção, eles se divertem filmando suas interações com o mundo espiritual. Até que Mia, uma jovem que perdeu a mãe, vai longe demais e abre uma porta permanente para o mundo espiritual. Sem saída, ela começa a ser assombrada por aparições malignas.",
                  "https://youtu.be/zIt5jSNPxNI?si=z4epE0_IqBhtPK-Y");





                /*Comédia*/
                await EnsureFilmAsync(
                    "Operação Natal", 2024, "Comédia", "12",
                    "https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcRooVp3qWP5b803QGT3l0U95_epTKSbJ0-3XBh1HNZ7dRfm--ry",
                    "Um vilão sequestra o Papai Noel do Polo Norte e um agente E.L.F. - Extremamente Grande e Formidável - une forças com o rastreador mais habilidoso do mundo para encontrá-lo e salvar o Natal.",
                    "https://youtu.be/ckhjh7X9x80?si=cQ4nMx5LzaGqUCH3");

                await EnsureFilmAsync(
                   "Os Farofeiros 2", 2024, "Comédia", "12",
                   "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcRGI592oscP9ad51CYutKAXV8Dawudm0MiHIrXVvK5saRNT-kX7",
                   "Os colegas de trabalho Alexandre, Lima, Rocha, Diguinho e suas famílias são presenteados pela empresa com uma viagem para a Bahia. No entanto, problemas e imprevistos podem levar por água abaixo essa viagem dos sonhos.",
                   "https://youtu.be/_d5quiHSmKE?si=3sg69PY1Z0XRnySO");

                await EnsureFilmAsync(
                   "Free Guy: Assumindo o Controle", 2021, "Comédia", "12",
                   "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcSR-vhHxUoTEifB2bTXPjfJKBb13Fr81MdJ1n8Xj0rClaKdoR_-",
                   "Um caixa de banco preso a uma entediante rotina tem sua vida virada de cabeça para baixo quando descobre que é um personagem em um jogo interativo. Ele precisa aceitar sua realidade e lidar com o fato de que é o único que pode salvar o mundo.",
                   "https://youtu.be/iLcwgsQ0V28?si=uruHK6ypywFKSZyW");

                await EnsureFilmAsync(
                   "Deadpool 2", 2018, "Comédia", "16",
                   "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQk4VujIOVqKQGdAk-MUHoGKTSyRHg8lzTnZsCpTuxOTle7BWP7",
                   "O supersoldado Cable vem do futuro com a missão de assassinar o jovem mutante Russel e o mercenário Deadpool precisa aprender o que é ser herói de verdade para salvá-lo. Para isso, ele recruta seu velho amigo Colossus e forma o novo grupo X-Force, sempre com o apoio do fiel escudeiro Dopinder.",
                   "https://youtu.be/a7t-avKlpkc?si=ZFkCC-bq7YZ1sJGt");

                await EnsureFilmAsync(
                  "Central de Inteligência", 2016, "Comédia", "14",
                  "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcQKCvEfHAy7d_VHdggX0hBDBAPVjBnuQK_e-0joxcPEmuiLJKmC",
                  "Antes de se tornar um agente da CIA, Bob sofria bullying na época do colégio. Na agência, ele precisa resolver um caso ultrassecreto e recorre a um antigo colega, popular nos tempos da escola, hoje contador.",
                  "https://youtu.be/Q_Zfna_MSBs?si=xwD4fV1B-wy1hR9d");


                await EnsureFilmAsync(
                  "Zerando a Vida", 2016, "Comédia", "16",
                  "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcThXrHrnsSuu9XRO5CjL6VsE2zIIKhCi7hvQ9hhEBtm8HOWV8kZ",
                  "Dois caras perdem todo o dinheiro e decidem fingir a própria morte, assumindo outras identidades. Entretanto, os novos nomes os colocam em apuros ainda maiores do que enfrentavam antes.",
                  "https://youtu.be/fWQ74OyrifY?si=4rtciQeQizi_dGEA");




                /*Animação*/
                await EnsureFilmAsync(
                    "O Espanta Tubarões", 2004, "Animação", "Livre",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTe82n27THQqz8s3OM6jNeQKcdK64Htj72ALIZMxhOXXRIuloWA",
                    "Um peixe se torna celebridade ao fingir ter matado um tubarão da máfia. Aliado ao irmão vegetariano da \"vítima\", ele desfruta da fama até que o mafioso Don Lino decide vingar a morte do filho, ameaçando expor sua mentira e acabar com sua vida.",
                    "https://youtu.be/dS9D869qMVg?si=anMy5psVf2TVTDkt");

                await EnsureFilmAsync(
                   "Zootopia: Essa Cidade é o Bicho", 2016, "Animação", "Livre",
                   "https://static.wikia.nocookie.net/dublagem/images/e/ea/Zootopia.jpg/revision/latest?cb=20250801034416&path-prefix=pt-br",
                   "Em uma cidade de animais, uma raposa falante se torna uma fugitiva ao ser acusada de um crime que não cometeu. O principal policial do local, o incontestável coelho, sai em sua busca.",
                   "https://youtu.be/prct6AB5tR8?si=lLhRiXZNSkaAnsnO");

                await EnsureFilmAsync(
                   "Up: Altas Aventuras", 2009, "Animação", "Livre",
                   "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcQibI4bWhWgHKSYGic70FJDt3fYBDUrANt_j5x4g9mK-wIoLxz1",
                   "Carl Fredricksen é um vendedor de balões que, aos 78 anos, está prestes a perder a casa em que sempre viveu com sua esposa, a falecida Ellie. Após um incidente, Carl é considerado uma ameaça pública e forçado a ser internado. Para evitar que isto aconteça, ele põe balões em sua casa, fazendo com que ela levante voo. Carl quer viajar para uma floresta na América do Sul, onde ele e Ellie sempre desejaram morar, mas descobre que um problema embarcou junto: Russell, um menino de 8 anos.",
                   "https://youtu.be/ODanPYf1VLI?si=fIYpqFvJ_YbR3pHW");

                await EnsureFilmAsync(
                    "Toy Story 5", 2026, "Animação", "Livre",
                    "https://m.media-amazon.com/images/M/MV5BM2MxMTNhNTQtZGJkOS00ZWRkLWIzZTctOTAyNWU3ZWNhMTRkXkEyXkFqcGc@._V1_.jpg",
                    "Toy Story 5 é um filme de animação digital do gênero comédia de aventura americano produzido pela Pixar Animation Studios para a Walt Disney Pictures. Dirigido e escrito por Andrew Stanton e codirigido por McKenna Harris, é o quinto filme da franquia Toy Story e a sequência de Toy Story 4.",
                    "https://youtu.be/AWRSlHuhNng?si=UllINgvtvWoEHJms");

                await EnsureFilmAsync(
                     "Por Água Abaixo", 2006, "Animação", "Livre",
                     "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcQ7KOMSxTAT-sBSPuhQgGwkF5Qq_vZOGQj0T58FkAdFEC4xtpDz",
                     "Roddy é um ratinho acostumado a um bairro luxuoso de Londres. Sem querer, ele dá uma descarga infeliz e acaba nos esgotos, onde terá de aprender a viver de uma forma completamente diferente.",
                     "https://youtu.be/UVzdqPepzng?si=D9OKvv0Bch38PmX_");

                await EnsureFilmAsync(
                    "Divertida Mente 2", 2024, "Animação", "Livre",
                    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTfG1TPdn9qevW-M5UEv-4F1BqgWm3VdTIdR9xM7H_pwGJ-BDzt",
                    "Com um salto temporal, Riley se encontra mais velha, passando pela tão temida adolescência. Junto com o amadurecimento, a sala de controle também está passando por uma adaptação para dar lugar a algo totalmente inesperado: novas emoções. As já conhecidas, Alegria, Raiva, Medo, Nojinho e Tristeza não têm certeza de como se sentir quando novos inquilinos chegam ao local.",
                    "https://youtu.be/yAZxx8t9zig?si=Q8Rd0Wl1-6zkf5zE");

                /*Romance*/
                await EnsureFilmAsync(
                    "Através da Minha Janela", 2022, "Romance", "16",
                    "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcRSdiW6ZyVKopBpdulh5Ri0le0Vb2UzmMysEkPBDnvh0NtDuL9w",
                    "Raquel é apaixonada pelo seu vizinho, Ares, um rapaz frio que vive em um mundo completamente diferente do seu. No entanto, o acaso acaba unindo os dois, que se veem envolvidos em uma trama de desejo e amor.",
                    "https://youtu.be/gyd3X62IcEM?si=-r_QP-EEGtwYcUg4");


                await EnsureFilmAsync(
                   "Amor e Outras Drogas", 2010, "Romance", "16",
                   "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcTmwiui6prSm8eF1mNhF4ZlNYjwuTmQVX3nus_z0rJS24OTOQnJ",
                   "Maggie é uma mulher de espírito livre que não quer se amarrar a alguém de maneira alguma. Ela só não esperava conhecer Jamie, um charmoso vendedor de produtos farmacêuticos que tem todas as mulheres aos seus pés. Aos poucos, o relacionamento evoluiu e ambos descobrem que estão sob a influência da droga mais forte já inventada: o amor.",
                   "https://youtu.be/DMQy0dbJI6c?si=s60YSdvd6RAkGPvY");


                await EnsureFilmAsync(
                  "Continência ao Amor", 2022, "Romance", "14",
                  "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcRPt4u44VFmK-wUsj_yqzCQoOocWnv8mWFGOHM8tOXNaicZUuvj",
                  "Uma cantora se casa por conveniência com um militar que está prestes a ir para a guerra, mas uma tragédia transforma esse relacionamento de fachada em realidade.",
                  "https://youtu.be/2yyzb9RDUgs?si=r32vyt5UUa5cDtwO");

                await EnsureFilmAsync(
                  "Sua Culpa", 2024, "Romance", "16",
                  "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQuFZHs60lOyQXTFllrKUumJIFkmxkbUDKB7YGaDujJ-TfAHvhv",
                  "O amor de Noah e Nick parece inabalável, apesar das tentativas dos pais de separá-los. Mas o trabalho dele e a entrada dela na faculdade abrem suas vidas para novos relacionamentos que irão abalar tanto o relacionamento dos dois quanto a família.",
                  "https://youtu.be/B2EX6qahZis?si=38tb2q4fQT6GFTav");

                await EnsureFilmAsync(
                  "Cinquenta Tons de Cinza", 2015, "Romance", "16",
                  "https://play-lh.googleusercontent.com/20obOlEzC0ppu2NuPs8ZXGhj-_cnJniZUaKkvbbIy8Q5htrM1q5OnTLlprAo9XCp8Ng",
                  "A estudante de literatura Anastasia Steele, de 21 anos, entrevista o jovem biolionário Christian Grey, como um favor a sua colega de quarto Kate Kavanagh. Ela vê nele um homem atraente e brilhante, e ele fica igualmente fascinado por ela.",
                  "https://youtu.be/DEwIt4amgq4?si=VfPgD6RgRQ7CV4S4");

                await EnsureFilmAsync(
                  "365 Dias", 2020, "Romance", "18",
                  "https://media.fstatic.com/dtCDL_LJBc4xtZH77b6nDjoJIuI=/350x525/smart/filters:format(webp)/media/movies/covers/2020/01/365_dni.jpg",
                  "Laura é uma diretora de vendas que embarca em uma viagem à Sicília para salvar seu relacionamento. Lá, ela conhece Massimo, um membro da máfia siciliana, que a sequestra e lhe dá 365 dias para se apaixonar por ele.",
                  "https://youtu.be/t_5LkTqtcTM?si=-v6liWD3tLESqw9w");


                /*Ação*/
                await EnsureFilmAsync(
                    "Resgate Implacável", 2025, "Ação", "16",
                    "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcRKSr1umHiBwKhf931iz1V2v1pkddMf7pUbcZo6AjiTt5laNR-d",
                    "Levon Cade deixou para trás uma carreira militar nas operações secretas para viver uma vida simples. No entanto, quando traficantes de pessoas sequestram a filha de seu chefe, sua busca para trazê-la de volta revela um mundo de corrupção muito maior do que ele poderia ter imaginado.",
                    "https://youtu.be/-HF5rHxs6vE?si=9rK1ljZGzW36xm5O");
               
                await EnsureFilmAsync(
                   "Velozes e Furiosos 5: Operação Rio", 2011, "Ação", "14",
                   "https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcTo3ITeQq2pZsUEsuo30L9sTU3bCnRR4qN5l_4CVkYGd-VskiP8",
                   "Desde que o ex-policial Brian O'Conner e Mia Toretto libertaram Dom da prisão, eles viajam pelo mundo para fugir das autoridades. No Rio de Janeiro, são obrigados a fazer um último trabalho antes de ganhar sua liberdade definitiva. Brian e Dom montam uma equipe de elite de pilotos de carro para executar a tarefa, mas precisam enfrentar um empresário corrupto e também um obstinado agente federal norte-americano.",
                   "https://youtu.be/8YCRGPaPfXA?si=qAdSOAyIMbJeT0Mh");


                await EnsureFilmAsync(
                  "Sem Remorso", 2021, "Ação", "16",
                  "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcQcV9Fdp1KF2ADHsvCkQz1tdPj8t99gdEbLqkev78ntZmFeJRcG",
                  "Um fuzileiro naval de elite descobre uma conspiração internacional enquanto busca justiça pelo assassinato de sua esposa grávida.",
                  "https://youtu.be/zDFgm9MGxHM?si=r_bb5oJ9D57Oosu4");

                await EnsureFilmAsync(
                  "Tropa de Elite", 2007, "Ação", "16",
                  "https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcR49CYQ7XWOfxWvS7oEfnyzrtA4l4y9AqB7XYdvIr3SwhRfEv4y",
                  "Nascimento, capitão da tropa de elite do Rio de Janeiro, é escolhido para chefiar uma das equipes cuja missão é apaziguar o Morro do Turano. Ele precisa cumprir as ordens enquanto procura por um substituto para ficar em seu lugar. Em meio a um tiroteio, Nascimento e sua equipe resgatam Neto e Matias, dois aspirantes a oficiais da PM. Ansiosos para entrar em ação, os dois se candidatam ao curso de formação do Batalhão de Operações Policiais Especiais.",
                  "https://youtu.be/_V_nZNWPYQk?si=07520BcTm2n5EsLw");

                await EnsureFilmAsync(
                  "Infiltrado", 2021, "Ação", "16",
                  "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcQr14HVRuqIeQU_AWigeSJPv8d8dICChMlGcZoaUVHaVA5hpp_G",
                  "Harry, conhecido apenas como H, é um homem misterioso que trabalha para uma empresa de carros-fortes e movimenta grandes quantias de dinheiro pela cidade de Los Angeles. Ao impedir um assalto, ele surpreende a todos com suas habilidades de combate. Suas verdadeiras intenções começam a ser questionadas e um plano maior é revelado.",
                  "https://youtu.be/sNP5HwaBK5I?si=ebHIeW7Y8HDbc2gh");

                await EnsureFilmAsync(
                  "Pecadores", 2024, "Ação", "16",
                  "https://encrypted-tbn2.gstatic.com/images?q=tbn:ANd9GcT-rGJvX_d5J5g_jMH-dR5wdqt_YrPIpAI6f-sdBnxrYf7_3N83",
                  "Dois irmãos gêmeos tentam deixar suas vidas problemáticas para trás e retornam à sua cidade natal para recomeçar. Lá, eles descobrem que um mal ainda maior está à espreita para recebê-los de volta.",
                  "https://youtu.be/e9kwQahD8YY?si=q_mk8e4OLHURY2h4");

            }

            if (ctx.ChangeTracker.HasChanges())
                await ctx.SaveChangesAsync();
        }
    }
}