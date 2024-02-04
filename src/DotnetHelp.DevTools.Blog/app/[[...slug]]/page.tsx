import { Metadata, ResolvingMetadata } from "next";
import CmsContentRender from "@/components/rich-text";
import { buildPageMenu, getPage, getPageMetadata } from "@/lib/page";
import Title from "@/components/title";

export default async function Page({ params }: { params: { slug: string[] } }) {
    const page = await getPage(params.slug?.join('/') || 'home');

    if (!page) {
        return <div>404</div>;
    }

    return <article>
        <Title title={page.title} overview={page.overview} />
        <CmsContentRender options={{ content: page.body.json }} />
    </article>
}

export async function generateStaticParams() {
    const result = await buildPageMenu();
    return result.map((p) => ({
        slug: [p.link],
    })).concat({ slug: [''] });
}

export async function generateMetadata({ params }: { params: { slug: string } }, parent: ResolvingMetadata): Promise<Metadata> {
    return await getPageMetadata(params.slug);
}