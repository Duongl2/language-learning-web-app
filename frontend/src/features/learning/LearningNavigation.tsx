import { NavLink } from 'react-router-dom';
import type { ComponentType } from 'react';

export const learningRoutes = import.meta.glob<{ path: string; title?: string; default: ComponentType }>('./routes/*.route.tsx', { eager: true });

export function LearningNavigation() {
  return <>{Object.values(learningRoutes).filter(route => route.title && !route.path.includes(':')).map(route => <NavLink key={route.path} to={route.path} className="block rounded-lg px-3 py-2 text-sm font-bold hover:bg-white">{route.title}</NavLink>)}</>;
}
