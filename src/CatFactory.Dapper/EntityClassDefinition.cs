﻿using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.Dapper
{
    public static class EntityClassDefinition
    {
        public static CSharpClassDefinition CreateEntity(this DapperProject project, ITable table)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");

            if (project.Settings.EnableDataBindings)
            {
                definition.Namespaces.Add("System.ComponentModel");

                definition.Implements.Add("INotifyPropertyChanged");

                definition.Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));
            }

            definition.Namespace = table.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(table.Schema);

            definition.Name = table.GetSingularName();

            definition.Constructors.Add(new ClassConstructorDefinition());

            var typeResolver = new ClrTypeResolver();

            if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
            {
                var column = table.PrimaryKey.GetColumns(table).First();

                definition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(typeResolver.Resolve(column.Type), column.GetParameterName()))
                {
                    Lines = new List<ILine>()
                        {
                            new CodeLine("{0} = {1};", column.GetPropertyName(), column.GetParameterName())
                        }
                });
            }

            if (!String.IsNullOrEmpty(table.Description))
            {
                definition.Documentation.Summary = table.Description;
            }

            foreach (var column in table.Columns)
            {
                if (project.Settings.EnableDataBindings)
                {
                    definition.AddViewModelProperty(typeResolver.Resolve(column.Type), column.GetPropertyName());
                }
                else
                {
                    if (project.Settings.UseAutomaticPropertiesForEntities)
                    {
                        definition.Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), column.GetPropertyName()));
                    }
                    else
                    {
                        definition.AddPropertyWithField(typeResolver.Resolve(column.Type), column.GetPropertyName());
                    }
                }
            }

            definition.Implements.Add("IEntity");

            if (project.Settings.SimplifyDataTypes)
            {
                definition.SimplifyDataTypes();
            }

            return definition;
        }

        public static CSharpClassDefinition CreateView(this DapperProject project, IView view)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");

            definition.Namespace = view.HasDefaultSchema() ? project.GetEntityLayerNamespace() : project.GetEntityLayerNamespace(view.Schema);

            definition.Name = view.GetSingularName();

            definition.Constructors.Add(new ClassConstructorDefinition());

            var typeResolver = new ClrTypeResolver();

            if (!String.IsNullOrEmpty(view.Description))
            {
                definition.Documentation.Summary = view.Description;
            }

            foreach (var column in view.Columns)
            {
                if (project.Settings.UseAutomaticPropertiesForEntities)
                {
                    definition.Properties.Add(new PropertyDefinition(typeResolver.Resolve(column.Type), column.GetPropertyName()));
                }
                else
                {
                    definition.AddPropertyWithField(typeResolver.Resolve(column.Type), column.GetPropertyName());
                }
            }

            definition.Implements.Add("IEntity");

            if (project.Settings.SimplifyDataTypes)
            {
                definition.SimplifyDataTypes();
            }

            return definition;
        }
    }
}
