1. Introducción
La industria del videojuego constituye uno de los sectores tecnológicos de mayor crecimiento en las últimas dos décadas, superando en volumen de negocio a las industrias del cine y la música combinadas. Desde el punto de vista de la ingeniería del software, el desarrollo de videojuegos plantea desafíos técnicos relevantes que abarcan arquitectura de sistemas en tiempo real, algoritmos de inteligencia artificial, generación procedural de contenido y diseño de interfaces interactivas. Este contexto convierte el desarrollo de videojuegos en un ámbito apropiado para la realización de un Trabajo de Fin de Grado (TFG) en Ingeniería Informática, donde se pueden aplicar y consolidar conocimientos adquiridos durante la formación académica.

1.1. Contexto y relevancia del problema
Los videojuegos del subgénero rogue-like presentan características técnicas que los convierten en un objeto de estudio especialmente interesante para la ingeniería del software. Se definen por la generación procedural de niveles, la ausencia de guardado de progreso entre partidas (permadeath), la progresión temporal dentro de cada ejecución y la alta rejugabilidad derivada de la variabilidad del contenido generado. Estos elementos obligan al desarrollador a diseñar sistemas robustos capaces de crear contenido coherente y equilibrado de forma automática, sin intervención manual.

El problema técnico fundamental que se plantea es el siguiente: ¿cómo se diseña e implementa una arquitectura modular que permita la generación automática de niveles jugables, la gestión concurrente de múltiples subsistemas (inteligencia artificial, física, interfaz de usuario, control de estados) y la parametrización de contenido de forma escalable y mantenible?

La vigencia de este problema se evidencia en la proliferación de títulos comerciales pertenecientes al género durante los últimos años (Hades, Dead Cells, The Binding of Isaac, Enter the Gungeon), lo que demuestra tanto el interés del público como la viabilidad técnica de este tipo de desarrollos. Desde la perspectiva académica, el estudio de estos sistemas permite explorar principios de arquitectura de software, patrones de diseño, algoritmos de generación procedural y técnicas de separación entre lógica y datos.

1.2. Propósito del trabajo
El propósito del presente TFG es diseñar, implementar y documentar un videojuego funcional perteneciente al subgénero rogue-like, utilizando Unity como motor gráfico y C# como lenguaje de programación. Se pretende demostrar la aplicabilidad de principios de ingeniería del software en un contexto práctico, con énfasis en la modularidad, la escalabilidad y la automatización del flujo de trabajo mediante herramientas de desarrollo.

El proyecto aborda tres áreas de conocimiento fundamentales:

Arquitectura de software: se aplican patrones de diseño como Singleton, Observer y Strategy para estructurar el código de forma modular. Se utiliza el concepto de ScriptableObject propio de Unity para separar lógica de juego y datos configurables, permitiendo modificar parámetros sin recompilar el proyecto.

Algoritmos y estructuras de datos: se implementan sistemas de generación procedural de salas mediante algoritmos deterministas, gestión de enemigos mediante listas ponderadas y estructuras de control de flujo basadas en máquinas de estados.

Ingeniería de herramientas: se desarrollan utilidades dentro del editor de Unity para automatizar tareas repetitivas (configuración de salas, validación de datos, generación masiva de contenido), reduciendo la probabilidad de error humano y acelerando el proceso de creación de activos.

1.3. Descripción del proyecto
El videojuego desarrollado, denominado Ascension, es un rogue-like bidimensional con vista cenital (top-down) en el que el jugador avanza a través de salas generadas de forma procedural, enfrentándose a enemigos hasta alcanzar la condición de victoria establecida: derrotar tres jefes que aparecen periódicamente cada cinco salas.

Se han implementado los siguientes sistemas:

Sistema de flujo de juego completo: menú principal, selección de tres clases de personaje con estadísticas diferenciadas, gestión de estados globales mediante GameManager y pantallas de victoria o derrota
Generación procedural de salas mediante tilemaps con muros perimetrales, puertas controlables y área jugable calculada dinámicamente
Sistema de combate con dos tipos de armas (cuerpo a cuerpo y a distancia), proyectiles físicos con detección de colisiones y generación procedural de estadísticas basada en rareza
Inteligencia artificial de enemigos con tres comportamientos diferenciados (persecución, disparo a distancia, aproximación por saltos) y un jefe con patrón de ataque en dos fases
Gestión de enemigos mediante generación ponderada por coste, limpieza automática de salas y control de apertura de puertas en función del estado de combate
Interfaz de usuario completa con visualización de salud, sistema de mensajes contextuales y tabla de puntuaciones persistente
Herramientas de automatización en el editor de Unity para configuración automática de salas, generación de mazmorras, gestión de bases de datos de enemigos y armas mediante interfaces gráficas, y aplicación de efectos sobre tiles
El proyecto no incluye sistema de audio ni controlador genérico de objetos recolectables.

1.4. Objetivos
El objetivo general del proyecto es desarrollar un videojuego rogue-like funcional que demuestre la aplicación de principios de ingeniería del software en un contexto de desarrollo de videojuegos, con énfasis en arquitectura modular, generación procedural y automatización de flujos de trabajo.

Los objetivos específicos son los siguientes:

Diseñar e implementar una arquitectura modular basada en patrones de diseño que permita la separación de responsabilidades entre subsistemas
Desarrollar un algoritmo de generación procedural de salas que garantice la jugabilidad y coherencia espacial del nivel
Implementar tres tipos de inteligencia artificial de enemigos con comportamientos diferenciados y un jefe con patrón de ataque en múltiples fases
Crear un sistema de armas con generación procedural de estadísticas y mecánicas de combate cuerpo a cuerpo y a distancia
Desarrollar herramientas de automatización dentro del editor de Unity para agilizar la creación y validación de contenido
Implementar un sistema de persistencia de puntuaciones mediante PlayerPrefs y visualización de ranking en el menú principal
Documentar el proceso de desarrollo, las decisiones técnicas adoptadas y las limitaciones encontradas
El presente documento expone el análisis, diseño e implementación detallada de cada uno de estos sistemas, así como las conclusiones extraídas del proceso de desarrollo.