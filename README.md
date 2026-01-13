
# 🎬 Movix — Backend API

## 📌 Sobre o Projeto

O **Movix** é um projeto **backend desenvolvido em C# com ASP.NET Core**, criado com foco total em **boas práticas de desenvolvimento**, organização de código e arquitetura profissional.

Este projeto representa uma aplicação real de conceitos fundamentais do desenvolvimento backend moderno, sendo pensado desde o início para ser **bem estruturado, legível, escalável e fácil de manter**. Todas as etapas do desenvolvimento foram cuidadosamente planejadas e documentadas.

---

## 🏗️ Arquitetura

O projeto foi construído seguindo uma abordagem de **Arquitetura em Camadas**, inspirada nos princípios da **Clean Architecture**, garantindo separação clara de responsabilidades.

```
Movix
├── Movix.Api             → Camada de entrada da aplicação (Controllers, Endpoints)
├── Movix.Application     → Casos de uso, regras de aplicação e DTOs
├── Movix.Domain          → Entidades e regras de negócio
├── Movix.Infrastructure → Persistência de dados e implementações técnicas
├── Movix.Web             → Camada de apresentação (quando aplicável)
```

### 🔹 Camadas do Sistema

* **Domain**
  Contém o núcleo da aplicação, com entidades e regras de negócio. Não depende de nenhuma outra camada.

* **Application**
  Responsável pelos fluxos da aplicação, validações e orquestração das regras de negócio.

* **Infrastructure**
  Implementa o acesso a dados, repositórios e recursos externos.

* **API**
  Exposição dos endpoints REST e comunicação entre o cliente e a aplicação.

---

## ⚙️ Tecnologias Utilizadas

### Backend

* **C#**
* **.NET / ASP.NET Core**
* **ASP.NET Core Web API**
* **Entity Framework Core**

### Arquitetura & Padrões

* Arquitetura em Camadas
* Princípios de Clean Architecture
* Injeção de Dependência
* DTOs para transferência de dados
* Separação de responsabilidades (SRP)

### Ferramentas

* **Visual Studio**
* **Git & GitHub**
* **Swagger** para documentação da API

---

## 📚 Organização e Qualidade do Código

* Código limpo e padronizado
* Estrutura clara por responsabilidade
* Nomes semânticos e consistentes
* Projeto pensado para fácil manutenção e evolução

Cada parte do sistema foi desenvolvida com atenção à qualidade e às boas práticas adotadas no mercado.

---

## 🚀 Objetivo do Projeto

O Movix tem como principal objetivo:

* Consolidar conhecimentos em **desenvolvimento backend com C# e .NET**
* Aplicar conceitos reais de arquitetura de software
* Servir como **projeto de portfólio profissional**
* Demonstrar capacidade de organização, documentação e boas práticas

---

## 👨‍💻 Autor

**Lucas**
Desenvolvedor de Software em formação
Desenvolvedor Web & Backend (.NET / C#)

---

⭐ Projeto desenvolvido com foco em qualidade, organização e aprendizado contínuo.
