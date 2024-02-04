import { Metadata, ResolvingMetadata } from "next";
import CmsContentRender from "@/components/rich-text";
import { getBlogMetadata, getBlogPost, getBlogPostOptions } from "@/lib/blog";
import Title from "@/components/title";

export default async function Blog({ params }: { params: { slug: string } }) {
    const page = await getBlogPost(params.slug);

    if (!page) {
        return <div>404</div>;
    }

    return <article>
        <Title 
            title={page.title} 
            overview={page.overview} 
            author={page.createdBy.name}
            date={page.createdAt}
            tags={page.tags}
        />
        <CmsContentRender options={{ content: page.body.json }} />
    </article>
}

export async function generateStaticParams() {
    const result = await getBlogPostOptions()
    return result.map((p) => ({
        slug: p.slug,
    }))
}

export async function generateMetadata({ params }: { params: { slug: string } }, parent: ResolvingMetadata): Promise<Metadata> {
    return await getBlogMetadata(params.slug);
}