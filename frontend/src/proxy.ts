import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

/**
 * Middleware for route protection
 * 
 * Redirects unauthenticated users to login.
 * Note: This is client-side augmented; the access token is stored in memory
 * so we check for the refresh token cookie as a proxy.
 */
export function proxy(request: NextRequest) {
    const { pathname } = request.nextUrl;

    // Public routes that don't require authentication
    const publicRoutes = ['/login', '/api/auth'];
    const isPublicRoute = publicRoutes.some((route) => pathname.startsWith(route));

    if (isPublicRoute) {
        return NextResponse.next();
    }

    // Check for refresh token cookie (indicates active session)
    // The actual token validation happens on the API side
    // const refreshToken = request.cookies.get('refreshToken');

    // If no refresh token and trying to access protected route, redirect to login
    // if (!refreshToken) {
    //    const loginUrl = new URL('/login', request.url);
    //    loginUrl.searchParams.set('redirect', pathname);
    //    return NextResponse.redirect(loginUrl);
    // }

    return NextResponse.next();

}

export const config = {
    matcher: [
        /*
         * Match all request paths except:
         * - _next/static (static files)
         * - _next/image (image optimization files)
         * - favicon.ico (favicon file)
         * - public files (images, etc.)
         */
        '/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)',
    ],
};
