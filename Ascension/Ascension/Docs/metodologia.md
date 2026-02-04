3. Metodología

El desarrollo del presente Trabajo de Fin de Grado se ha basado en un enfoque iterativo e incremental, orientado a la construcción progresiva de un sistema software funcional mediante la implementación y validación continua de sus distintos componentes. Este enfoque ha permitido adaptar el alcance del proyecto conforme se consolidaban las decisiones técnicas, manteniendo en todo momento la estabilidad del sistema y la coherencia global de la arquitectura.

La elección de esta metodología resulta especialmente adecuada para un proyecto de desarrollo de videojuegos, donde la experimentación, el ajuste de mecánicas y la validación temprana de funcionalidades son factores clave para garantizar la viabilidad del producto final.

3.1. Tipo de investigación

El trabajo se enmarca dentro de una investigación de tipo aplicado, centrada en el diseño e implementación de un producto software funcional. El objetivo principal es demostrar la aplicación práctica de conceptos de ingeniería del software en el contexto del desarrollo de videojuegos, más concretamente en la creación de un videojuego 2D del subgénero rogue-like.

No se ha realizado un estudio empírico con participantes externos ni se han aplicado técnicas formales de recolección de datos cuantitativos, dado que el objeto de estudio es el propio sistema desarrollado y su correcto funcionamiento desde un punto de vista técnico y arquitectónico.

3.2. Metodología de desarrollo de software

Se ha adoptado una metodología de desarrollo iterativa e incremental, basada en la implementación progresiva de funcionalidades completas y su validación continua. Esta elección se justifica por la naturaleza exploratoria del proyecto y por la necesidad de redefinir determinados aspectos del diseño conforme avanzaba el desarrollo.

El proceso de trabajo se ha estructurado en iteraciones de corta duración, cada una de las cuales ha generado un incremento funcional del sistema. En cada iteración se han llevado a cabo las siguientes actividades: diseño de un subsistema concreto, implementación de la funcionalidad asociada, validación mediante pruebas funcionales en el editor de Unity, corrección de errores detectados y ajuste del alcance de la iteración siguiente en función de los resultados obtenidos.

No se ha aplicado una metodología formal basada en marcos de trabajo ágiles como Scrum o Kanban, dado que el proyecto se ha desarrollado de forma individual y sin un equipo de trabajo. La gestión del desarrollo se ha realizado de manera autónoma, priorizando los subsistemas críticos para el funcionamiento del videojuego y adaptando la planificación a las limitaciones técnicas identificadas durante el proceso.

3.3. Proceso de implementación y validación

La implementación del sistema se ha llevado a cabo siguiendo un proceso secuencial de definición de subsistemas, codificación, pruebas funcionales y refinamiento. Cada componente del videojuego se ha desarrollado de forma modular e independiente y posteriormente se ha integrado en el sistema global, verificando su correcto funcionamiento conjunto con los subsistemas ya existentes.

El proceso de validación se ha basado en pruebas funcionales manuales ejecutadas directamente en el editor de Unity. En cada iteración se han verificado aspectos clave del sistema, entre los que se incluyen: la generación procedural de salas con muros y puertas funcionales, el comportamiento de la inteligencia artificial de enemigos con distintos patrones de actuación, la aplicación correcta de efectos de baldosas sobre las entidades, el flujo de avance entre salas, la aparición periódica de jefes según el número de salas completadas y la activación de la condición de victoria tras la derrota del tercer jefe.

No se ha implementado un conjunto formal de pruebas automatizadas ni se ha realizado un proceso de pruebas de usuario con participantes externos. La validación se ha centrado en garantizar la estabilidad funcional del sistema, la ausencia de errores críticos y la coherencia del flujo de juego.

3.4. Herramientas de desarrollo y automatización

El desarrollo del proyecto se ha realizado utilizando Unity versión 6000.0.31f1 como motor de desarrollo y el lenguaje de programación C# para la implementación de la lógica del sistema. Como entorno de desarrollo integrado (IDE) se ha empleado Visual Studio 2022, y Git se ha utilizado como sistema de control de versiones.

De forma complementaria, se han desarrollado herramientas específicas integradas en el editor de Unity con el objetivo de automatizar tareas repetitivas y facilitar la creación y validación de contenido. Entre estas herramientas se incluyen: generadores automáticos de configuraciones de sala con validación de integridad de datos, editores visuales para bases de datos de enemigos y armas, sistemas de aplicación masiva de efectos sobre baldosas y utilidades de depuración para verificar el estado interno del flujo de juego.

El uso de estas herramientas ha permitido reducir el tiempo dedicado a tareas de configuración manual y disminuir la aparición de errores derivados de inconsistencias en los datos del sistema.

3.5. Técnicas de diseño y organización del código

Se ha aplicado una arquitectura modular basada en la separación de responsabilidades entre subsistemas. Los datos del videojuego se han definido mediante objetos de tipo ScriptableObject, desacoplando la información relativa a enemigos, armas y clases de jugador de la lógica de ejecución del sistema. Esta decisión ha facilitado la parametrización del contenido y ha simplificado la incorporación de nuevas variantes sin necesidad de modificar el código fuente.

Se han empleado patrones de diseño orientados a la gestión del flujo de juego, incluyendo un controlador global de estado (GameManager) y controladores específicos para la gestión de salas (RoomFlowController). La comunicación entre componentes se ha realizado, cuando ha sido necesario, mediante eventos y delegados, evitando dependencias directas innecesarias entre objetos.

La estructura del proyecto se ha organizado en carpetas temáticas dentro del directorio Assets, separando scripts, datos, prefabricados, escenas y recursos gráficos. Esta organización ha facilitado el mantenimiento del código y la comprensión de la arquitectura del sistema a lo largo del desarrollo.

3.6. Documentación del proceso

El proceso de desarrollo se ha documentado de forma continua mediante archivos en formato Markdown almacenados en el directorio Docs del proyecto. En esta documentación se ha mantenido un registro de las decisiones técnicas adoptadas, las limitaciones identificadas y el estado funcional de los principales subsistemas.

No se elaboró una especificación formal de requisitos ni diagramas de casos de uso detallados en las fases iniciales del proyecto, dado que el alcance se definió de manera progresiva. No obstante, una vez consolidada la implementación del sistema, se han generado diagramas de clases y descripciones técnicas de los subsistemas principales, que se incluyen como parte de la documentación final del trabajo.